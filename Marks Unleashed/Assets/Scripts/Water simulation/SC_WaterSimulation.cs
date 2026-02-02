using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Timeline;
using UnityEngine.UIElements;


public struct Entry : IComparable<Entry>
{
    public int particleIndex;
    public uint cellKey;

    public Entry(int particleIndex, uint cellKey)
    {
        this.particleIndex = particleIndex;
        this.cellKey = cellKey;
    }

    public int CompareTo(Entry other)
    {
        return cellKey.CompareTo(other.cellKey);
    }
}


public class SC_WaterSimulation : MonoBehaviour
{
    static readonly ThreadLocal<System.Random> rng =
    new ThreadLocal<System.Random>(() => new System.Random());

    [Header("References")]
    public Mesh particleMesh;
    public Material particleMaterial;

    [Header("Particles")]
    public float particleSize;
    [Range(0, 1)]
    public float collisionDamping = 1;
    [Range(0.9f, 1)]
    public float velocityDamping = 0.99f;
    public float smoothingRadius = 1;
    public float mass = 1;
    public float target_density = 0;
    public float pressure_multiplier = 350;
    public float viscosity = 0.1f;
    public float restDensity = 10;
    public float max_color_speed = 2;

    private float selfDensity = 0;

    [Header("World stats")]
    public float gravity = 1;
    public float particleSpacing = 1;
    public int particleAmount;
    public Vector3 boundsSize;
    public bool randomizeStartPos = false;


    Matrix4x4[] matrices;
    Vector3[] positions;
    Vector3[] predicted_positions;
    Vector3[] velocities;
    float[] particleProperties;
    public float[] densities;
    Vector4[] colors;
    float cellSize;
    int[] startIndices;

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

    struct spatialLookupStruct
    {
        public uint cellKey;
    }
    Entry[] spatialLookup;

    private bool canStart = false;

    MaterialPropertyBlock mpb;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Initialize variables
        mpb = new MaterialPropertyBlock();

        positions = new Vector3[particleAmount];
        predicted_positions = new Vector3[particleAmount];
        velocities = new Vector3[particleAmount];
        particleProperties = new float[particleAmount];
        densities = new float[particleAmount];
        matrices = new Matrix4x4[particleAmount];
        colors = new Vector4[particleAmount];
        startIndices = new int[particleAmount];

        cellSize = smoothingRadius;
        spatialLookup = new Entry[particleAmount];

        target_density = mass * 10;
        selfDensity = mass * SmoothKernel(smoothingRadius, 0);

        // Place particles in a grid
        int particlesPerRow = (int)Mathf.Sqrt(particleAmount);
        int particlesPerCol = (particleAmount - 1) / particlesPerRow + 1;
        float spacing = (particleSize * 0.01f) * 2 + particleSpacing;

        for (int i = 0; i < particleAmount; i++)
        {
            float x = 0, y = 0, z = 0;
            if (randomizeStartPos)
            {
                Vector3 size = boundsSize / 2 - Vector3.one * (particleSize * 0.01f);
                x = (float)(UnityEngine.Random.value - 0.5f) * boundsSize.x;
                y = (float)(UnityEngine.Random.value - 0.5f) * boundsSize.y;
                z = (float)(UnityEngine.Random.value - 0.5f) * boundsSize.z;
            }
            else
            {
                x = (i % particlesPerRow - particlesPerRow / 2 + 0.5f) * spacing;
                y = (i % particlesPerCol - particlesPerCol / 2 + 0.5f) * spacing;
                z = (i % particlesPerRow - particlesPerRow / 2 + 0.5f) * spacing;
            }

            positions[i] = new Vector3(x, y, z);
            predicted_positions[i] = positions[i];
            particleProperties[i] = Mathf.Cos(y - 3 + Mathf.Sin(x) + Mathf.Sin(z));
            matrices[i] = Matrix4x4.TRS(positions[i], Quaternion.identity, Vector3.one * particleSize);
        }

        UpdateDensities();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            canStart = !canStart;

        SimulationStep(Time.deltaTime);


        for (int i = 0; i < particleAmount; i++)
        {
            if (!IsFinite(positions[i]))
                continue;

            float speed = velocities[i].magnitude;
            float t = Mathf.InverseLerp(0, max_color_speed, speed);

            colors[i] = SpeedToColor(t);
        }

        mpb.Clear();
        mpb.SetVectorArray("_Color", colors);

        Graphics.DrawMeshInstanced(
            particleMesh,
            0,
            particleMaterial,
            matrices,
            particleAmount,
            mpb
        );
        Graphics.DrawMeshInstanced(particleMesh, 0, particleMaterial, matrices, particleAmount, mpb);
    }

    Vector4 SpeedToColor(float t)
    {
        if (t < 0.33f)
            return Color.Lerp(Color.blue, Color.cyan, t / 0.33f);
        if (t < 0.66f)
            return Color.Lerp(Color.cyan, Color.yellow, (t - 0.33f) / 0.33f);
        return Color.Lerp(Color.yellow, Color.red, (t - 0.66f) / 0.34f);
    }

    bool IsFinite(Vector3 v)
    {
        return float.IsFinite(v.x) &&
               float.IsFinite(v.y) &&
               float.IsFinite(v.z);
    }

    public void UpdateSpatialLookup(Vector3[] points, float radius)
    {
        this.positions = points;

        Parallel.For(0, points.Length, i =>
        {
            (int cellX, int cellY, int cellZ) = PositionToCellCoord(points[i], radius);
            uint cellKey = GetKeyFromHash(HashCell(cellX, cellY, cellZ));
            spatialLookup[i] = new Entry(i, cellKey);
            startIndices[i] = int.MaxValue; // Reset start index
        });

        // Sort by cell key
        Array.Sort(spatialLookup, (a, b) => a.cellKey.CompareTo(b.cellKey));

        Parallel.For(0, points.Length, i =>
        {
            uint key = spatialLookup[i].cellKey;
            uint keyPrev = i == 0 ? uint.MaxValue : spatialLookup[i - 1].cellKey;
            if (keyPrev != key)
            {
                startIndices[key] = i;
            }
        });
    }

    // Converts a position to the coordinate of the cell it's within
    public (int x, int y, int z) PositionToCellCoord(Vector3 point, float radius)
    {
        int cellX = (int)(point.x / radius);
        int cellY = (int)(point.y / radius);
        int cellZ = (int)(point.z / radius);

        return (cellX, cellY, cellZ);
    }
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
    public uint GetKeyFromHash(uint hash)
    {
        return hash % (uint)spatialLookup.Length;
    }
    public void ForeachPointWithinRadius(Vector3 point, Action<int> action)
    {
        (int centerX, int centerY, int centerZ) = PositionToCellCoord(point, smoothingRadius);
        float sqrRadius = smoothingRadius * smoothingRadius;

        for (int i = 0; i < offsets3D.Length; i++)
        {
            Vector3Int offset = offsets3D[i];
            uint key = GetKeyFromHash(HashCell(centerX + offset.x, centerY + offset.y, centerZ + offset.z));
            int cellStartIndex = startIndices[key];

            for (int j = cellStartIndex; j < spatialLookup.Length; j++)
            {
                if (spatialLookup[j].cellKey != key) break; /// Breaks the loop if it doesn't look at the same cell

                int particleIndex = spatialLookup[j].particleIndex;

                float sqrDist = (positions[particleIndex] - point).sqrMagnitude;

                if (sqrDist <= sqrRadius)
                {
                    action(particleIndex);
                }
            }
        }
    }

    private void ApplyBordersPressure(int index, float bound_limit, ref Vector3 pressureForce)
    {
        float ground = bound_limit * 0.5f + particleSize * 0.01f;
        float penetration = ground + smoothingRadius - predicted_positions[index].y;

        if (penetration > 0f)
        {
            float p = ConvertDensityToPressure(densities[index]);
            pressureForce += Vector3.up * p * penetration;
        }
    }

    void SimulationStep(float deltaTime)
    {
        UpdateSpatialLookup(positions, smoothingRadius);

        // Apply gravity and calculate the density
        Parallel.For(0, particleAmount, i =>
        {
            densities[i] = CalculateDensity(i);
        });
        // Calculate and apply the forces
        Parallel.For(0, particleAmount, i =>
        {
            Vector3 pressureForce = CalculatePressureForce(i);
            Vector3 viscosityForce = CalculateViscosityForce(i);

            // Ground boundary pressure
            ApplyBordersPressure(i, -boundsSize.y, ref pressureForce);
            ApplyBordersPressure(i, -boundsSize.x, ref pressureForce);
            ApplyBordersPressure(i, boundsSize.x, ref pressureForce);
            ApplyBordersPressure(i, -boundsSize.z, ref pressureForce);
            ApplyBordersPressure(i, boundsSize.z, ref pressureForce);


            Vector3 acceleration = (pressureForce + viscosityForce) / densities[i] + Vector3.down * gravity;

            velocities[i] += acceleration * deltaTime;
            velocities[i] *= velocityDamping;
            predicted_positions[i] = positions[i] + velocities[i] * 1 / 120f;

            Debug.DrawRay(
                positions[i],
                pressureForce * 0.001f,
                Color.green
            );
        });
        // Update positions and resolve collisions
        Parallel.For(0, particleAmount, i =>
        {
            positions[i] += velocities[i] * deltaTime;
            ResolveCollisions(ref positions[i], ref velocities[i]);
            matrices[i].SetTRS(positions[i], Quaternion.identity, Vector3.one * particleSize);
        });
    }
    void UpdateDensities()
    {
        Parallel.For(0, particleAmount, i =>
        {
            densities[i] = CalculateDensity(i);
        });
    }

    private void ResolveCollisions(ref Vector3 point, ref Vector3 velocity)
    {
        Vector3 halfBoundsSIze = boundsSize / 2 - Vector3.one * (particleSize * 0.01f);

        if (Mathf.Abs(point.x) > halfBoundsSIze.x)
        {
            point.x = halfBoundsSIze.x * Mathf.Sign(point.x);
            velocity.x *= -1 * collisionDamping;
        }
        if (Mathf.Abs(point.y) > halfBoundsSIze.y)
        {
            point.y = halfBoundsSIze.y * Mathf.Sign(point.y);
            velocity.y *= -1 * collisionDamping;
        }
        if (Mathf.Abs(point.z) > halfBoundsSIze.z)
        {
            point.z = halfBoundsSIze.z * Mathf.Sign(point.z);
            velocity.z *= -1 * collisionDamping;
        }
    }

    Color PressureToColor(float p)
    {
        return Color.Lerp(Color.blue, Color.red, p);
    }
    float NormalizePressure(float p)
    {
        float min = 0, max = pressure_multiplier;
        return Mathf.InverseLerp(min, max, p);
    }
    float ConvertDensityToPressure(float density)
    {
        float densityError = density - restDensity;
        return Mathf.Max(densityError * pressure_multiplier, 0);
    }

    Vector3 GetRandomDirection()
    {
        Vector3[] directions = { Vector3.down, Vector3.up, Vector3.forward, Vector3.back };
        return directions[rng.Value.Next(0, directions.Length - 1)];

    }

    Vector3 CalculatePressureForce(int partIndex)
    {
        Vector3 pressureForce = Vector3.zero;
        Vector3 point = predicted_positions[partIndex];

        float part_pressure = ConvertDensityToPressure(densities[partIndex]);

        ForeachPointWithinRadius(point, j =>
        {
            if (j == partIndex) return;

            Vector3 offset = point - predicted_positions[j];
            float dist = offset.magnitude;
            if (dist <= 0 || dist >= smoothingRadius) return;

            float other_density = densities[j];
            float other_pressure = ConvertDensityToPressure(other_density);
            
            Vector3 gradient = SpikyKernelGradient(offset, dist);
            pressureForce += -mass * (part_pressure + other_pressure) / (2 * other_density) * gradient;
            //pressureForce += gradient;
        });

        return pressureForce;
    }
    Vector3 CalculateViscosityForce(int partIndex)
    {
        Vector3 viscosityForce = Vector3.zero;

        Vector3 partPoint = predicted_positions[partIndex];
        Vector3 partVel = velocities[partIndex];

        ForeachPointWithinRadius(partPoint, index =>
        {
            if (index == partIndex) return;

            Vector3 offset = predicted_positions[index] - partPoint;
            float dist = offset.magnitude;

            if (dist >= smoothingRadius) return;

            float influence = SmoothKernel(smoothingRadius, dist);

            viscosityForce += viscosity * (velocities[index] - partVel) * influence * mass / densities[index];
        });

        return viscosityForce;
    }

    float CalculateSharedPressure(float densityA, float densityB)
    {
        // Newton's third law
        float pA = ConvertDensityToPressure(densityA);
        float pB = ConvertDensityToPressure(densityB);
        return (pA + pB) / 2;
    }
    float CalculateProperty(Vector3 point)
    {
        float property = 0;
        for (int i = 0; i < particleAmount; i++)
        {
            float dist = (positions[i] - point).magnitude;
            float influence = SmoothKernel(smoothingRadius, dist); /// get the influence the particle has at the point
            float density = CalculateDensity(0);
            property += particleProperties[i] * influence * mass / density;
        }
        return property;
    }
    float CalculateDensity(int particleIndex)
    {
        float density = selfDensity;
        Vector3 point = predicted_positions[particleIndex];

        ForeachPointWithinRadius(point, neighborIndex =>
        {
            if (neighborIndex == particleIndex) return;

            float dist = (predicted_positions[neighborIndex] - point).magnitude;
            float influence = SmoothKernel(smoothingRadius, dist);
            density += mass * influence;
        });

        return density;
    }

    /// <summary>
    /// Calculates how close two particles are, and in wich direction and with how many force they should pressure push
    /// </summary>
    private Vector3 SpikyKernelGradient(Vector3 r, float dist)
    {
        if (dist >= smoothingRadius || dist <= 0) return Vector3.zero; /// Avoids calculating far particles and dividing by 0

        float coeff = -45 / (Mathf.PI * Mathf.Pow(smoothingRadius, 6));
        float factor = coeff * Mathf.Pow(smoothingRadius - dist, 2);

        return factor * (r / dist);
    }
    private float SmoothKernel(float radius, float dist)
    {
        if (dist >= radius) return 0;
        float volume = Mathf.PI * Mathf.Pow(radius, 4) / 6; /// Normalize volume so that a higher smoothing range doesn't increase the density

        return (radius - dist) * (radius - dist) / volume;
    }
    private float SmoothKernelDerivative(float radius, float dist)
    {
        if (dist >= radius) return 0;

        float scale = 12 / (Mathf.Pow(radius, 4) * Mathf.PI);
        return (dist - radius) * scale;
    }



    // Input
    Vector3 InteractionForce(Vector3 inputPos, float radius, float strength, int particleIndex)
    {
        Vector3 interactionForce = Vector3.zero;
        Vector3 offset = inputPos - positions[particleIndex];
        float sqrDist = Vector3.Dot(offset, offset);

        if (sqrDist < radius * radius)
        {
            float dist = Mathf.Sqrt(sqrDist);
            Vector3 dirToInputPoint = dist <= float.Epsilon ? Vector3.zero : offset / dist;
            float center = 1 - dist / radius;
            interactionForce += (dirToInputPoint * strength - velocities[particleIndex]) * center;
        }

        return interactionForce;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(Vector3.zero, boundsSize);
    }
}