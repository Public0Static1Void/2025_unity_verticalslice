using System;
using System.Threading;
using System.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEditor.PlayerSettings;

public class SC_WaterSimulation2 : MonoBehaviour
{
    static readonly ThreadLocal<System.Random> rng =
    new ThreadLocal<System.Random>(() => new System.Random());



    [Header("References")]
    public Mesh particleMesh;
    public Material particleMaterial;
    [Header("World stats")]
    public bool randomizeStartPos = false;
    public int particleAmount;
    public float gravity;
    public Vector3 boxSize;
    public float radius;

    public float targetDensity = 10;
    public float recommendedDensity = 10;
    public float pressureMultiplier = 1, nearPressureMultiplier = 1;

    private Vector3 _boxStartPosition;

    public int subSteps = 1;
    
    [Header("Particles")]
    public float particleSize;
    [Range(0, 1)]
    public float collisionDamping = 0.75f;
    [Range(0.9f, 1)]
    public float velocityDamping = 0.999f;
    public float viscorityStrength = 1;
    public float smoothingRadius = 1;
    public float mass = 1;


    // Particles data
    private Matrix4x4[] _particleMatrix;
    private Vector3[] _positions;
    private Vector3[] _predictedPositions;
    private Vector3[] _velocities;
    [SerializeField]
    private float[] _densities;
    private Vector4[] _colors;


    private float[] particleProperty;

    private MaterialPropertyBlock _mpb;

    // Optimizations
    private uint[] _cells;
    private Entry[] _spatialLookup;
    private int[] _startIndicies; // Saves the start index of every cell to avoid extra calculations

    [Header("Renderer")]
    public bool show_density = true;
    public Color col_low_speed = Color.cyan;
    public Color col_max_speed = Color.red;
    public float max_speed = 5, min_speed = 2;
    public float mouseForce = 50, mouseRange = 10;

    Camera cam;



    static readonly Vector3Int[] offsets3D =
    {
        new Vector3Int(-1, -1, -1),
        new Vector3Int(-1, -1, 0),
        new Vector3Int(-1, -1, 1),
        new Vector3Int(-1, 0, -1),
        new Vector3Int(-1, 0, 0),
        new Vector3Int(-1, 0, 1),
        new Vector3Int(-1, 1, -1),
        new Vector3Int(-1, 1, 0),
        new Vector3Int(-1, 1, 1),
        new Vector3Int(0, -1, -1),
        new Vector3Int(0, -1, 0),
        new Vector3Int(0, -1, 1),
        new Vector3Int(0, 0, -1),
        new Vector3Int(0, 0, 0),
        new Vector3Int(0, 0, 1),
        new Vector3Int(0, 1, -1),
        new Vector3Int(0, 1, 0),
        new Vector3Int(0, 1, 1),
        new Vector3Int(1, -1, -1),
        new Vector3Int(1, -1, 0),
        new Vector3Int(1, -1, 1),
        new Vector3Int(1, 0, -1),
        new Vector3Int(1, 0, 0),
        new Vector3Int(1, 0, 1),
        new Vector3Int(1, 1, -1),
        new Vector3Int(1, 1, 0),
        new Vector3Int(1, 1, 1)
    };




    void Start()
    {
        cam = Camera.main;
        _mpb = new MaterialPropertyBlock();

        // Particles
        _particleMatrix = new Matrix4x4[particleAmount];
        _positions = new Vector3[particleAmount];
        _predictedPositions = new Vector3[particleAmount];
        _velocities = new Vector3[particleAmount];
        _densities = new float[particleAmount];
        _colors = new Vector4[particleAmount];

        // Optimization
        _cells = new uint[particleAmount];
        _spatialLookup = new Entry[particleAmount];
        _startIndicies = new int[_spatialLookup.Length];

        particleProperty = new float[particleAmount];

        int particlesPerRow = (int)Mathf.Sqrt(particleAmount);
        int particlesPerCol = (particleAmount - 1) / particlesPerRow + 1;
        float spacing = (particleSize * 0.01f) * 2 + 1;

        _boxStartPosition = transform.position;

        // 1. Initialize positions first
        for (int i = 0; i < particleAmount; i++)
        {
            float x, y, z;

            if (randomizeStartPos)
            {
                x = (UnityEngine.Random.value - 0.5f) * boxSize.x;
                y = (UnityEngine.Random.value - 0.5f) * boxSize.y;
                z = (UnityEngine.Random.value - 0.5f) * boxSize.z;
            }
            else
            {
                x = (i % particlesPerRow - particlesPerRow / 2 + 0.5f);
                y = (i % particlesPerCol - particlesPerCol / 2 + 0.5f);
                z = (i % particlesPerCol - particlesPerRow / 2 + 0.5f);
            }

            _positions[i] = new Vector3(x, y, z);
        }

        _predictedPositions = _positions;

        // 2. Sample the field for each particle
        for (int i = 0; i < particleAmount; i++)
        {
            _particleMatrix[i] = Matrix4x4.TRS(
                _positions[i],
                Quaternion.identity,
                Vector3.one * particleSize
            );

            _colors[i] = Color.cyan;
        }

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            for (int i = 0; i < particleAmount; i++)
            {
                Vector3 inputForce = InteractionForce(GetMouseWorldPosition(transform.position.z), mouseRange, mouseForce, i);
                _velocities[i] += (inputForce / _densities[i]) * Time.deltaTime;
            }
        }
        else if (Input.GetMouseButton(1))
        {
            for (int i = 0; i < particleAmount; i++)
            {
                Vector3 inputForce = InteractionForce(GetMouseWorldPosition(transform.position.z), mouseRange, -mouseForce, i);
                _velocities[i] += (inputForce / _densities[i]) * Time.deltaTime;
            }
        }

        float delta = Time.deltaTime / subSteps;
        for (int i = 0; i < subSteps; i++)
        {
            UpdatePhysics(delta);
        }

        float idealSpacing = smoothingRadius * 0.55f;   // 0.5–0.65 range
        recommendedDensity = mass / (idealSpacing * idealSpacing * idealSpacing);

        _mpb.Clear();

        for (int i = 0; i < particleAmount; i++)
        {
            float basis = show_density ? _densities[i] : _velocities[i].magnitude;
            float t = Mathf.InverseLerp(min_speed, max_speed, basis);
            Vector4 col = Color.Lerp(col_low_speed, col_max_speed, t);
            _colors[i] = col;
        }
        _mpb.SetVectorArray("_Color", _colors);


        for (int i = 0; i < particleAmount; i++)
        {
            Vector3 pos = _positions[i];
            if (float.IsNaN(pos.x) || float.IsNaN(pos.y) || float.IsNaN(pos.z) ||
                float.IsInfinity(pos.x) || float.IsInfinity(pos.y) || float.IsInfinity(pos.z))
            {
                Debug.LogWarning($"Particle {i} has invalid position: {pos} – resetting!");
                _positions[i] = Vector3.zero;
                _velocities[i] = Vector3.zero;
                _particleMatrix[i].SetTRS(_positions[i], Quaternion.identity, Vector3.one * particleSize);
            }
        }

        Graphics.DrawMeshInstanced(particleMesh, 0, particleMaterial, _particleMatrix, particleAmount, _mpb);
    }

    void UpdatePhysics(float deltaTime)
    {

        // Apply gravity and calculate current density
        Parallel.For(0, particleAmount, i =>
        {
            _velocities[i] += Vector3.down * gravity * deltaTime;
            _predictedPositions[i] = _positions[i] + _velocities[i] * deltaTime;
        });

        UpdateSpatialLookup(_predictedPositions, smoothingRadius);

        Parallel.For(0, particleAmount, i =>
        {
            _densities[i] = CalculateDensity(_predictedPositions[i]);
        });

        // Calculate and apply the pressure forces
        Parallel.For(0, particleAmount, i =>
        {
            Vector3 pressureForce = CalculatePressureForce(i);
            Vector3 viscosityForce = CalculateViscosityForce(i);
            // F = acc * masa -> Aceleración = fuerza / densidad (Densidad es masa por volumen, por lo que se puede poner aquí)
            Vector3 acceleration = (pressureForce + viscosityForce) / _densities[i];
            if (acceleration.magnitude > 500) // Clamp acceleration
            {
                acceleration = acceleration.normalized * 500;
            }
            _velocities[i] += acceleration * deltaTime;
            _velocities[i] *= velocityDamping;
        });

        Parallel.For(0, particleAmount, i =>
        {
            _positions[i] += _velocities[i] * deltaTime;
            _particleMatrix[i].SetTRS(_positions[i], Quaternion.identity, Vector3.one * particleSize);
            HandleColision(ref _positions[i], ref _velocities[i]);
        });
    }

    private void UpdateSpatialLookup(Vector3[] points, float radius)
    {
        this.radius = radius;

        // Create the unordered spatial lookup
        Parallel.For(0, points.Length, i =>
        {
            (int x, int y, int z) = PositionToCellCoord(points[i], radius);
            uint cellKey = GetKeyFromHash(HashCell(x, y, z));

            _spatialLookup[i] = new Entry(i, cellKey);
            _startIndicies[i] = int.MaxValue;
        });

        // Sort by cell key
        Array.Sort(_spatialLookup);

        // Calculate the start indices of each cell in the spatial lookup
        Parallel.For(0, points.Length, i =>
        {
            uint key = _spatialLookup[i].cellKey;
            uint prevKey = i == 0 ? uint.MaxValue : _spatialLookup[i - 1].cellKey;
            if (key != prevKey)
            {
                _startIndicies[key] = i;
            }
        });
    }

    public void ForEachPointWithinRadius(Vector3 point, Action<int> action)
    {
        var (centerX, centerY, centerZ) = PositionToCellCoord(point, smoothingRadius);
        float sqrRadius = smoothingRadius * smoothingRadius;

        for (int ox = -1; ox <= 1; ox++)
            for (int oy = -1; oy <= 1; oy++)
                for (int oz = -1; oz <= 1; oz++)
                {
                    int cx = centerX + ox;
                    int cy = centerY + oy;
                    int cz = centerZ + oz;

                    uint key = GetKeyFromHash(HashCell(cx, cy, cz));

                    if (key >= _startIndicies.Length) continue; // safety

                    int start = _startIndicies[key];
                    if (start == int.MaxValue) continue; // empty cell

                    for (int j = start; j < _spatialLookup.Length; j++)
                    {
                        if (_spatialLookup[j].cellKey != key) break;

                        int idx = _spatialLookup[j].particleIndex;
                        Vector3 diff = _predictedPositions[idx] - point;
                        float sqrDist = diff.sqrMagnitude;

                        if (sqrDist < sqrRadius && sqrDist > 1e-8f) // avoid self + div0
                        {
                            action(idx);
                        }
                    }
                }
    }

    private void HandleColision(ref Vector3 pos, ref Vector3 vel)
    {
        Vector3 _halfBox = boxSize / 2 - Vector3.one * (particleSize * 0.01f);
        if (Mathf.Abs(pos.y) > _halfBox.y)
        {
            vel.y *= -1 * collisionDamping;
            pos.y = (_boxStartPosition.y + _halfBox.y) * Mathf.Sign(pos.y);
        }
        if (Mathf.Abs(pos.x) > _halfBox.x)
        {
            vel.x *= -1 * collisionDamping;
            pos.x = (_boxStartPosition.x + _halfBox.x) * Mathf.Sign(pos.x);
        }
        if (Mathf.Abs(pos.z) > _halfBox.z)
        {
            vel.z *= -1 * collisionDamping;
            pos.z = (_boxStartPosition.z + _halfBox.z) * Mathf.Sign(pos.z);
        }
    }

    Vector3 GetRandomDirection()
    {
        Vector3[] directions = { Vector3.down, Vector3.up, Vector3.forward, Vector3.back };
        return directions[rng.Value.Next(0, directions.Length - 1)];

    }

    // Boundaries
    void ApplyBoundaryPressure(int i, ref Vector3 pressure)
    {
        Vector3 p = _predictedPositions[i];
        Vector3 half = boxSize * 0.5f;

        // Influence distance — usually 0.8–1.5 × smoothingRadius
        float h = smoothingRadius * 1.0f;

        // Linear stiffness (repulsion grows linearly with penetration)
        float k = 2000;         // Tune: 2000–15000

        // Quadratic/extra near term — helps a lot with stacking & prevents sinking
        float k_near = 30000;   // Tune: 8000–30000, often 2–5× k

        // ──────────────────────────────────────────────
        // Left wall (x = -half.x)
        if (p.x < -half.x + h)
        {
            float penetration = (-half.x + h) - p.x;           // > 0 when inside influence
            float forceMag = penetration * k + penetration * penetration * k_near;
            pressure += Vector3.right * forceMag;              // push to +x
        }

        // Right wall (x = +half.x)
        if (p.x > half.x - h)
        {
            float penetration = p.x - (half.x - h);            // > 0 when too far right
            float forceMag = penetration * k + penetration * penetration * k_near;
            pressure += Vector3.left * forceMag;               // push to -x
        }

        // Bottom wall (y = -half.y)  ← most important for your settling problem
        if (p.y < -half.y + h)
        {
            float penetration = (-half.y + h) - p.y;
            float forceMag = penetration * k + penetration * penetration * k_near;
            pressure += Vector3.up * forceMag;                 // push upward
        }

        // Top wall (y = +half.y)
        if (p.y > half.y - h)
        {
            float penetration = p.y - (half.y - h);
            float forceMag = penetration * k + penetration * penetration * k_near;
            pressure += Vector3.down * forceMag;
        }

        // Back wall (z = -half.z)
        if (p.z < -half.z + h)
        {
            float penetration = (-half.z + h) - p.z;
            float forceMag = penetration * k + penetration * penetration * k_near;
            pressure += Vector3.forward * forceMag;            // +z direction
        }

        // Front wall (z = +half.z)
        if (p.z > half.z - h)
        {
            float penetration = p.z - (half.z - h);
            float forceMag = penetration * k + penetration * penetration * k_near;
            pressure += Vector3.back * forceMag;               // -z direction
        }
    }


    #region Optimization
    // Optimizing functions
    public static uint HashCell(int x, int y, int z)
    {
        unchecked /// Wraps math overflows
        {
            return (uint)(
                (x * 73856093) ^
                (y * 19349663) ^
                (z * 83492791)
            );
        }
    }
    public (int x, int y, int z) PositionToCellCoord(Vector3 point, float radius)
    {
        int cellX = (int)(point.x / radius);
        int cellY = (int)(point.y / radius);
        int cellZ = (int)(point.z / radius);

        return (cellX, cellY, cellZ);
    }
    public uint GetKeyFromHash(uint hash)
    {
        return hash % (uint)_spatialLookup.Length;
    }
    #endregion

    #region Physics
    // Rare physic functions
    Vector3 CalculateViscosityForce(int partIndex)
    {
        Vector3 force = Vector3.zero;
        Vector3 myVel = _velocities[partIndex];
        ForEachPointWithinRadius(_predictedPositions[partIndex], i =>
        {
            Vector3 relVel = _velocities[i] - myVel;
            float dist = (_predictedPositions[i] - _predictedPositions[partIndex]).magnitude;
            float influence = SmoothKernel(smoothingRadius, dist);
            force += relVel * influence;
        });
        return force * viscorityStrength * 2f;  // increase strength
    }
    Vector3 CalculatePressureForce(int partIndex)
    {
        Vector3 pressure = Vector3.zero;
        ApplyBoundaryPressure(partIndex, ref pressure);

        ForEachPointWithinRadius(_predictedPositions[partIndex], otherIndex =>
        {
            if (otherIndex == partIndex) return;

            // Direction
            Vector3 offset = _predictedPositions[otherIndex] - _predictedPositions[partIndex];
            float dist = offset.magnitude;
            Vector3 dir = dist == 0 ? GetRandomDirection() : offset / dist;


            float slope = SpikyKernelDerivative(smoothingRadius, dist);

            float densityOther = Mathf.Max(_densities[otherIndex], 1e-5f);

            float sharedPressure = CalculateSharedPressure(_densities[partIndex], _densities[otherIndex]);
            pressure += sharedPressure * dir * slope * mass / densityOther;
        });

        return pressure;
    }

    float CalculateSharedPressure(float densA, float densB)
    {
        float pA = ConvertDensityToPressure(densA); // Pressure A
        float pB = ConvertDensityToPressure(densB); // Pressure B
        float nA = ConvertNearPressure(densA);   // ← now quadratic
        float nB = ConvertNearPressure(densB);

        return (pA + pB) * 0.5f + (nA + nB) * 0.5f;
    }
    float ConvertDensityToPressure(float density)
    {
        float densityError = density - targetDensity;
        float pressure = densityError * pressureMultiplier;
        return pressure;
    }
    float ConvertNearPressure(float density)
    {
        float error = density - targetDensity;
        if (error <= 0) return 0;

        return nearPressureMultiplier * error * error * error;
    }
    float CalculateDensity(Vector3 point)
    {
        float density = mass * SpikyKernel(smoothingRadius, 0f); // self contribution

        ForEachPointWithinRadius(point, i =>
        {
            float dist = (_predictedPositions[i] - point).magnitude;
            density += mass * SpikyKernel(smoothingRadius, dist);
        });

        return Mathf.Max(density, 1e-5f);
    }

    // Kernel
    private float SmoothKernel(float radius, float dist)
    {
        if (dist >= radius) return 0;
        float volume = (Mathf.PI * Mathf.Pow(radius, 4)) / 6; /// Normalize volume so that a higher smoothing range doesn't increase the density

        return (radius - dist) * (radius - dist) / volume;
    }
    private float SmoothKernelDerivative(float radius, float dist)
    {
        if (dist >= radius) return 0;

        float scale = 12 / (Mathf.Pow(radius, 4) * Mathf.PI);
        return (dist - radius) * scale;
    }
    private float SpikyKernel(float radius, float dist)
    {
        if (dist >= radius) return 0;
        float q = dist / radius;
        return (15f / (Mathf.PI * radius * radius * radius)) * Mathf.Pow(1f - q, 3f);
    }
    private float SpikyKernelDerivative(float radius, float dist)
    {
        if (dist >= radius || dist < 1e-6f) return 0;
        float q = dist / radius;

        return - (45f / (Mathf.PI * radius * radius * radius * radius)) *Mathf.Pow(1f - q, 2f);
    }
    #endregion


    // Input
    Vector3 InteractionForce(Vector3 inputPos, float radius, float strength, int partIndex)
    {
        Vector3 force = Vector3.zero;

        Vector3 offset = inputPos - _positions[partIndex];
        float sqrDist = Vector3.Dot(offset, offset);

        if (sqrDist < radius * radius)
        {
            float dist = Mathf.Sqrt(sqrDist);
            Vector3 dirToInputPoint = dist <= float.Epsilon ? Vector3.zero : offset / dist;
            float center = 1 - dist / radius;

            force += (dirToInputPoint * strength - _velocities[partIndex]) * center;
        }

        return force;
    }
    Vector3 GetMouseWorldPosition(float fixedZ)
    {
        Vector3 mouseScreenPos = Input.mousePosition;

        // Distance from camera to the plane Z = fixedZ
        float distance = fixedZ - cam.transform.position.z;

        mouseScreenPos.z = distance;

        return cam.ScreenToWorldPoint(mouseScreenPos);
    }


    private void OnDrawGizmos()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawWireCube(transform.position, boxSize);
    }
}