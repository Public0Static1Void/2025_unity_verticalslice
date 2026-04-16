using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SC_PlayerLife : SC_LifeSystem
{
    [Header("Stats")]
    public float recover_time = 5;
    public Color vignette_color;

    [Header("Scene")]
    public string reload_scene = "SampleScene";

    [Header("References")]
    public GameObject applied_elements_parent;
    private List<Image> applied_elements_images = new List<Image>();

    public Volume camera_volume;
    private Vignette camera_vignette = null;


    private bool recovering_health = false;

    private Coroutine routine_color = null;

    private float target_intensity = 0;
    public Color target_color = Color.white;


    private void Start()
    {
        // Load volume component
        camera_volume.profile.TryGet<Vignette>(out Vignette v);
        if (v != null)
        {
            camera_vignette = v;
            camera_vignette.intensity.value = 0;
        }
        else
            Debug.LogWarning("Vignette not added to the volume camera");
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.L))
            Damage(1);

        // Lerp the camera vignette values
        if (!SC_GameManager.AreColorsSimilar(camera_vignette.color.value, target_color, 0.0001f))
        {
            camera_vignette.intensity.value = Mathf.Lerp(camera_vignette.intensity.value, target_intensity, Time.deltaTime * 10);
            camera_vignette.color.value = Color.Lerp(camera_vignette.color.value, target_color, Time.deltaTime * 10);
        }
        else
        {
            target_color = Color.white;
            Debug.Log("Color restored");
        }
            
    }

    public override void UpdateAppliedElementsUI()
    {
        return;
        for (int i = 0; i < applied_elements.Length; i++)
        {
            Sprite sprite = null;
            if (applied_elements[i] != null)
            {
                sprite = applied_elements[i].im_mark_sprite;
                applied_elements_images[i].gameObject.SetActive(true);
            }
            else
            {
                applied_elements_images[i].gameObject.SetActive(false);
            }

            applied_elements_images[i].sprite = sprite;
        }
    }
    public override void Damage(float damage)
    {
        base.Damage(damage);

        SC_CameraRotation.instance.ShakeCamera(0.25f, 0.25f);

        float vignette_value = 1 - ((float)life / (float)max_life);
        target_intensity = vignette_value * 0.75f;
        target_color = vignette_color;

        RecoverHealth();
    }


    private void RecoverHealth()
    {
        if (recovering_health)
        {
            StopCoroutine("RecoverHealthRoutine");
            SetVignetteValue(max_life - life, vignette_color);
        }

        recovering_health = true;
        StartCoroutine(RecoverHealthRoutine());
    }

    private IEnumerator RecoverHealthRoutine()
    {
        yield return new WaitForSeconds(10); // Wait two seconds to start healing

        float lifePerSecond = (max_life - life) / recover_time; // Get how many life to give for second

        float current_life = life;

        float timer = 0;
        while (timer < recover_time)
        {
            current_life += lifePerSecond * Time.deltaTime;
            life = Mathf.FloorToInt(current_life);

            SetVignetteValue(max_life - current_life, vignette_color);

            timer += Time.deltaTime;
            yield return null;
        }

        recovering_health = false;
    }

    public void SetVignetteValue(float newValue, Color newColor, bool priority = false)
    {
        target_intensity = newValue;
        target_color = newColor;
    }


    public override void Die()
    {
        Debug.Log("Dead x)");
        SceneManager.LoadScene(reload_scene);
    }
}