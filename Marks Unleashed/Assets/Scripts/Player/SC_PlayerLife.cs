using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SC_PlayerLife : SC_LifeSystem
{
    [Header("Stats")]
    public float recover_time = 5;
    public float vignette_duration;
    public Color vignette_color;

    [Header("Scene")]
    public string reload_scene = "SampleScene";

    [Header("References")]
    public GameObject applied_elements_parent;
    private List<Image> applied_elements_images = new List<Image>();

    public Volume camera_volume;
    private Vignette camera_vignette = null;

    public Image image_vignette;

    private bool recovering_health = false;

    private Coroutine routine_damage_effect = null;

    private float target_intensity = 0;
    public Color target_color = Color.white;

    [Header("Materials")]
    [SerializeField] private Material fire_damage_material;
    [SerializeField] private Material ice_damage_material;

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
    private IEnumerator Routine_damage_effect(float duration, Material damageMat)
    {
        float timer = 0;

        while (timer < duration)
        {
            float t = timer / duration;
            t = Mathf.SmoothStep(0, 1, t);
            float softnessValue = Mathf.Lerp(0, 2, t);
            damageMat.SetFloat("_Softness", softnessValue);
            
            timer += Time.deltaTime;
            yield return null;
        }

        timer = 0;
        while (timer < duration)
        {
            float t = timer / duration;
            t = Mathf.SmoothStep(0, 1, t);
            float softnessValue = Mathf.Lerp(2, 0, t);
            damageMat.SetFloat("_Softness", softnessValue);

            timer += Time.deltaTime;
            yield return null;
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
    public override void Damage(float damage, Damage_type damage_type = Damage_type.NORMAL)
    {
        base.Damage(damage);

        SC_CameraRotation.instance.ShakeCamera(0.25f, 0.25f);

        float vignette_value = 1 - ((float)life / (float)max_life);
        target_intensity = vignette_value * 0.75f;
        target_color = vignette_color;

        Material mat = null;
        switch(damage_type)
        {
            case Damage_type.NORMAL:
                mat = ice_damage_material; //Recordar cambiar material daño normal
                break;

            case Damage_type.FIRE:
                mat = new Material (fire_damage_material); 
                break;

            case Damage_type.ICE:
                mat = new Material (ice_damage_material);
                break;
        }
        image_vignette.material = mat;

        StartCoroutine(Routine_damage_effect(vignette_duration, mat));
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
        return;
        target_intensity = newValue;
        target_color = newColor;
    }


    public override void Die()
    {
        SceneManager.LoadScene(reload_scene);
    }
}