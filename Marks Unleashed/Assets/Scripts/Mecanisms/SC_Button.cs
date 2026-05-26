using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Events;

[RequireComponent(typeof(BoxCollider))]
[RequireComponent(typeof(AudioSource))]
public class SC_Button : MonoBehaviour
{
    [Header("Normal")]
    public List<SC_Interactable> attached_mecanisms = new List<SC_Interactable>();
    public UnityEvent event_normal = new UnityEvent();
    [Header("Inverse")]
    public List<SC_Interactable> attached_mecanisms_inverse = new List<SC_Interactable>();
    public UnityEvent event_inverse = new UnityEvent();

    [Header("References")]
    public Animator anim_button;
    [Space(10)]

    private List<Material> light_emitter = new List<Material>();
    public List<MeshRenderer> light_meshes = new List<MeshRenderer>();

    protected AudioSource audioSource;
    public AudioClip clip_connected;

    

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.clip = clip_connected;
        foreach (MeshRenderer m in light_meshes)
        {
            if (m == null) continue;
            m.material.EnableKeyword("_EMISSION");
            light_emitter.Add(m.material);
        }
    }

    private void SetMatsLight(Color col, float intensity)
    {
        for (int i = 0; i < light_emitter.Count; i++)
        {
            Material mat = light_meshes[i].material; // IMPORTANT

            if (intensity > 0f)
            {
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", col * intensity);
            }
            else
            {
                mat.SetColor("_EmissionColor", Color.black);
                mat.DisableKeyword("_EMISSION");
            }
        }
    }

    public virtual void ActivateMecanisms()
    {
        anim_button.SetBool("active", true);

        for (int i = 0; i <  attached_mecanisms.Count; i++)
        {
            attached_mecanisms[i].Activate();
            event_normal?.Invoke();
        }
        for (int i = 0; i < attached_mecanisms_inverse.Count; i++)
        {
            attached_mecanisms_inverse[i].Deactivate();
            event_inverse?.Invoke();
        }

        SetMatsLight(Color.white, 5);

        audioSource.Play();
    }
    public virtual void DeactivateMecanisms()
    {
        anim_button.SetBool("active", false);

        for (int i = 0; i < attached_mecanisms.Count; i++)
        {
            attached_mecanisms[i].Deactivate();
            event_inverse?.Invoke();
        }
        for (int i = 0; i < attached_mecanisms_inverse.Count; i++)
        {
            attached_mecanisms_inverse[i].Activate();
            event_normal?.Invoke();
        }

        SetMatsLight(Color.white, 0);
    }
}