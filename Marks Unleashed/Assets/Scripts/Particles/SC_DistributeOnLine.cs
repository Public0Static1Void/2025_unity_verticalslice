using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class SC_DistributeOnLine : MonoBehaviour
{
    public float length = 1;
    public int particle_num = 20;
    public float time_between_parts = 0.015f;
    public float check_distance = 5;
    public float particle_fall_speed = 1;
    public float distance_between_particles;

    public Vector2 max_offset;

    private ParticleSystem _ps;

    private float[] particle_duration;
    private Vector3[] particle_target_pos;

    private float[] particle_float;
    private bool[] particle_no_float;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _ps = GetComponent<ParticleSystem>();

        particle_duration = new float[particle_num];
        particle_float = new float[particle_num];
        particle_no_float = new bool[particle_num];
        particle_target_pos = new Vector3[particle_num];

        StartCoroutine(InitPartsLine());
    }

    private IEnumerator InitPartsLine()
    {
        _ps.Play();

        Vector3 current_location = transform.position + transform.right * transform.localScale.x * _ps.shape.radius;
        Vector3 start_location = current_location + Vector3.up;

        float distance = (transform.localScale.x * _ps.shape.radius * 2) / particle_num;

        
        for (int current_part = 0; current_part < particle_num; current_part++)
        {
            // Spawn the particle
            var emit_params = new ParticleSystem.EmitParams();
            emit_params.position = current_location;
            emit_params.velocity = Vector3.zero;
            emit_params.startSize = Random.Range(2, 5);

            _ps.Emit(emit_params, 1);

            Vector3 diff = current_location - start_location;
            Vector3 dir = diff.normalized;
            float dist = diff.magnitude + 1;
            if (Physics.Raycast(start_location, dir, out RaycastHit hit, dist))
            {
                Debug.DrawRay(start_location, dir * dist, Color.green, 5);
                particle_target_pos[current_part] = hit.point;
                particle_no_float[current_part] = false;
            }
            else
            {
                Debug.DrawRay(start_location, dir * dist, Color.red, 5);
                particle_target_pos[current_part] = current_location;
                particle_float[current_part] = Random.Range(0, Mathf.PI * 2);
                particle_no_float[current_part] = true;
            }

            particle_duration[current_part] = time_between_parts * current_part;

            Vector3 baseDir = -transform.right;

            // Create a small random offset in LOCAL space
            Vector3 randomOffset = new Vector3(
                Random.Range(-max_offset.x, max_offset.x),
                0,
                Random.Range(-max_offset.y, max_offset.y)
            );

            // Convert offset to world space properly
            Vector3 finalDir = (baseDir + transform.TransformDirection(randomOffset)).normalized;

            current_location += finalDir * distance;

            yield return new WaitForSeconds(time_between_parts);
        }

        StartCoroutine(AnimatePartsLineRoutine());
    }

    private IEnumerator AnimatePartsLineRoutine()
    {
        ParticleSystem.Particle[] parts = new ParticleSystem.Particle[particle_num];

        int particleCount = _ps.GetParticles(parts);

        // Save the original spawn position of every particle
        Vector3[] parts_start_pos = new Vector3[particleCount];
        for (int i = 0; i < particleCount; i++)
        {
            parts_start_pos[i] = parts[i].position;
        }

        float timer = 0f;

        while (true)
        {
            particleCount = _ps.GetParticles(parts);

            for (int i = 0; i < particleCount; i++)
            {
                var part = parts[i];

                Vector3 basePosition;

                // Move the particles on the ground to the hit position
                if (!particle_no_float[i])
                {
                    float duration = Mathf.Max(particle_duration[i], 0.0001f);
                    float progress = Mathf.Clamp01(timer / duration);

                    basePosition = Vector3.Lerp(
                        parts_start_pos[i],
                        particle_target_pos[i],
                        progress
                    );
                }
                else
                {
                    basePosition = parts_start_pos[i];
                }

                // Makes particles on the air a float like movement
                if (particle_no_float[i])
                {
                    float frequency = 1f;   // Oscillation speed
                    float amplitude = 0.15f; // Vertical movement range

                    float yOffset = Mathf.Sin(
                        timer * frequency + particle_float[i]
                    ) * amplitude;

                    part.position = basePosition + Vector3.up * yOffset;
                }
                else
                {
                    part.position = basePosition;
                }

                parts[i] = part;
            }

            _ps.SetParticles(parts, particleCount);

            timer += Time.deltaTime;
            yield return null;
        }
    }
}
