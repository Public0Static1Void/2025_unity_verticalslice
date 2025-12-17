using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

[RequireComponent(typeof(BoxCollider))]
[RequireComponent(typeof(AudioSource))]
public class SC_Button : MonoBehaviour
{

    public List<SC_Interactable> attached_mecanisms = new List<SC_Interactable>();
    public List<SC_Interactable> attached_mecanisms_inverse = new List<SC_Interactable>();

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
        for (int i = 0; i <  attached_mecanisms.Count; i++)
        {
            attached_mecanisms[i].Activate();
            if (i < attached_mecanisms_inverse.Count)
                attached_mecanisms_inverse[i].Deactivate();
        }

        SetMatsLight(Color.white, 5);

        audioSource.Play();
    }
    public virtual void DeactivateMecanisms()
    {
        for (int i = 0; i < attached_mecanisms.Count; i++)
        {
            attached_mecanisms[i].Deactivate();
            if (i < attached_mecanisms_inverse.Count)
                attached_mecanisms_inverse[i].Activate();
        }

        SetMatsLight(Color.white, 0);
    }
}