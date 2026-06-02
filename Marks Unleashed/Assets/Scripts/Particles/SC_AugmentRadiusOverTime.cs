using System.Collections;
using UnityEngine;

public class SC_AugmentRadiusOverTime : MonoBehaviour
{
    public float duration = 1;
    public float max_radius = 1;
    public float time_between_waves = 0.25f;
    public Vector2 size;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(AugmentRadiusRoutine(transform.parent != null ? transform.parent : transform));
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private IEnumerator AugmentRadiusRoutine(Transform parentTransform)
    {
        ParticleSystem ps = GetComponent<ParticleSystem>();
        ps.Play();

        float timer = 0f;
        float wave_timer = 0f;
        float spawnRate = 200; // particles per wave
        float accumulator = 0f;
        float lastWaveRadius = 0f;

        int current_wave = 0;
        float total_waves = duration / time_between_waves;

        while (current_wave <= total_waves)
        {
            float dt = Time.deltaTime;
            timer += dt;
            wave_timer += dt;

            float t = timer / duration;
            float radius = Mathf.Lerp(0f, max_radius, t);

            if (wave_timer > time_between_waves)
            {
                accumulator += spawnRate;

                while (accumulator >= 1f)
                {
                    Vector3 parentScale = parentTransform.lossyScale;

                    // --- New: growing annulus ---
                    float minWaveRadius = lastWaveRadius;
                    float maxWaveRadius = radius;
                    float r = Mathf.Sqrt(Random.value * (maxWaveRadius * maxWaveRadius - minWaveRadius * minWaveRadius) + minWaveRadius * minWaveRadius);
                    Vector2 dir = UnityEngine.Random.insideUnitCircle.normalized;
                    Vector2 offset = dir * r + new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f));

                    var emitParams = new ParticleSystem.EmitParams();
                    emitParams.position = parentTransform.TransformPoint(new Vector3(offset.x / parentScale.x, 0, offset.y / parentScale.z));
                    emitParams.velocity = Vector3.zero;
                    emitParams.startSize = Random.Range(size.x, size.y);

                    ps.Emit(emitParams, 1);

                    accumulator -= 1f;
                }

                lastWaveRadius = radius; // remember the outer radius for the next wave
                wave_timer = 0;
                current_wave++;
            }

            yield return null;
        }
    }
}
