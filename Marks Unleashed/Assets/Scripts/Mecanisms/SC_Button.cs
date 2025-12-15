using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
[RequireComponent(typeof(AudioSource))]
public class SC_Button : MonoBehaviour
{
    public float activation_mass = 5;
    public List<SC_Interactable> attached_mecanisms = new List<SC_Interactable>();

    private bool activated = false;

    private GameObject activation_for = null;

    private List<Material> light_emitter = new List<Material>();
    public List<MeshRenderer> light_meshes = new List<MeshRenderer>();

    private AudioSource audioSource;
    public AudioClip clip_connected;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.clip = clip_connected;
        foreach (MeshRenderer m in light_meshes)
        {
            m.material.EnableKeyword("_EMISSION");
            light_emitter.Add(m.material);
        }
    }

    public void ActivateMecanisms()
    {
        audioSource.Play();

        for (int i = 0; i < attached_mecanisms.Count; i++)
        {
            attached_mecanisms[i].Activate();
        }
        foreach (Material mat in light_emitter)
        {
            Color emissionColor = Color.white * 5;

            mat.SetColor("_EmissionColor", emissionColor);
        }
    }
    public void DeactiavteMecanisms()
    {

        for (int i = 0; i < attached_mecanisms.Count; i++)
        {
            attached_mecanisms[i].Deactivate();
        }

        foreach (Material mat in light_emitter)
        {
            Color emissionColor = Color.white * 0;

            mat.SetColor("_EmissionColor", emissionColor);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (activated || activation_for != null) return; /// Don't activate again if it's pressed

        Rigidbody rb = other.GetComponent<Rigidbody>();
        if (rb != null && rb.mass >= activation_mass)
        {
            ActivateMecanisms();
            activation_for = other.gameObject;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        /// Check if the object is the one that pressed the button
        if (other.gameObject == activation_for)
        {
            DeactiavteMecanisms();
            activation_for = null;
            activated = false;
        }
    }
}