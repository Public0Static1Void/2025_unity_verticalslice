using UnityEngine;

public class SC_FluctuateLight : MonoBehaviour
{
    private Light _light;
    [Header("Intensity")]
    public float fluctuationAmount = 0.5f;
    public float speed = 5;
    public bool activate_delay = false;

    public float start_intensity = 0;

    private float _timer = 0;

    public bool use_start_intensity = false;

    void Start()
    {
        _light = GetComponent<Light>();
        if (use_start_intensity)
        {
            start_intensity = _light.intensity;
        }
    }

    void Update()
    {
        if (activate_delay)
        {
            _timer += Time.deltaTime;
            if (_timer > 3)
            {
                activate_delay = false;
            }
        }
        else
        {
            // Creates a smooth 'random' fluctuation
            float noise = Mathf.PerlinNoise(Time.time * speed, 0f);
            noise = (noise - 0.5f) * 2f;

            _light.intensity = start_intensity + noise * fluctuationAmount;
        }
    }
}
