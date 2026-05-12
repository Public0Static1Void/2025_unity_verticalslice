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

    private bool isBurning = false;
    private Coroutine burnEffectCoroutine = null;

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
            //Debug.Log("Color restored");
        }
            
    }

    private IEnumerator BurnEffectCoroutine(float duration, Material damageMat)
    {
        isBurning = true;
        float timer = 0;

        //Fade IN
        float fadeInTime = 0.3f;
        float t = 0;
        while (t < fadeInTime)
        {
            float lerp = t / fadeInTime;
            float softnessValue = Mathf.Lerp(4, 0.7f, lerp);
            damageMat.SetFloat("_Softness", softnessValue);
            image_vignette.SetMaterialDirty();

            t += Time.deltaTime;
            yield return null;
        }

        damageMat.SetFloat("_Softness", 0.7f);

        //Mantener efecto mientras dura burn
        while (timer < duration)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        //Fade OUT
        t = 0;
        while(t < fadeInTime)
        {
            float lerp = t / fadeInTime;
            float softnessValue = Mathf.Lerp(0.7f, 4f, lerp);
            damageMat.SetFloat("_Softness", softnessValue);
            image_vignette.SetMaterialDirty();

            t += Time.deltaTime;
            yield return null;
        }

        damageMat.SetFloat("_Softness", 4f);

        isBurning = false;
    }

    public void StartBurnEffect(float duration)
    {
        if (isBurning)
            return;

        Material instanceMat = new Material(fire_damage_material);
        image_vignette.material = instanceMat;

        burnEffectCoroutine = StartCoroutine(BurnEffectCoroutine(duration, instanceMat));
    }

    private IEnumerator Routine_damage_effect(float duration, Material damageMat)
    {
        if (damageMat == null)
        {
            Debug.LogError("Material pasado a la corutina es null");
            yield break;
        }
        Debug.Log("Corutina Routine_damage_effect INICIADA (Fade In)");
        // Fade in
        float timer = 0;

        while (timer < duration)
        {
            float t = Mathf.SmoothStep(0f, 1f, timer / duration);
            //t = Mathf.SmoothStep(0, 1, t);
            float softnessValue = Mathf.Lerp(4f, 0.7f, t);

            damageMat.SetFloat("_Softness", softnessValue);
            image_vignette.SetMaterialDirty();
            
            Debug.Log($"Timer: {timer:F2} | t: {t:F2} | Softness set to: {softnessValue:F2} | GetFloat: {damageMat.GetFloat("_Softness"):F2}");
            
            timer += Time.deltaTime;
            yield return null;
        }
        damageMat.SetFloat("_Softness", 0.7f);

        Debug.Log("Fade In terminado, empezando Fade Out");
        //Fade Out
        timer = 0f;
        while (timer < duration)
        {
            float t = Mathf.SmoothStep(0f, 1f, timer / duration);
            float softnessValue = Mathf.Lerp(0.7f, 4f, t);

            damageMat.SetFloat("_Softness", softnessValue);
            image_vignette.SetMaterialDirty();

            Debug.Log($"FADE OUT Timer: {timer:F2} | t: {t:F2} | Softness > {softnessValue:F2} | Get: {damageMat.GetFloat("_Softness"):F2}");

            timer += Time.deltaTime;
            yield return null;
        }
        damageMat.SetFloat("_Softness", 4f);

        Debug.Log("Corutina Routine_damage_effect TERMINADA");
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
        Debug.Log("Damage() llamado - Recibido daño: " + damage + " | Tipo: " + damage_type);

        base.Damage(damage);

        SC_CameraRotation.instance.ShakeCamera(0.25f, 0.25f);


        if (damage_type == Damage_type.NORMAL)
        {
            float vignette_value = 1 - ((float)life / (float)max_life);
            target_intensity = vignette_value * 0.75f;
            target_color = vignette_color;
            return;
        }

        Material originalMat = null;
        switch(damage_type)
        {
            //Si ya se está quemando no spammear vignette
            case Damage_type.FIRE:
                if (isBurning)
                    return;

                originalMat = fire_damage_material; 
                break;

            case Damage_type.ICE:
                originalMat = ice_damage_material;
                break;
        }

        if (originalMat == null)
        {
            Debug.LogError($"El material para el tipo {damage_type} no está asignado en el Inspector");
            return;
        }

        Material instanceMat = new Material(originalMat);
        image_vignette.material = instanceMat;
        
        Debug.Log($" Material instancia creado y asignado: {instanceMat.name}");

        StartCoroutine(Routine_damage_effect(vignette_duration, instanceMat));
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
        SC_PlayerRespawn respawnScript = GetComponent<SC_PlayerRespawn>();
        respawnScript.Respawn();
    }
}