using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class SC_Button : MonoBehaviour
{
    public float activation_mass = 5;
    public List<SC_Interactable> attached_mecanisms = new List<SC_Interactable>();

    private bool activated = false;

    private GameObject activation_for = null;

    public void ActivateMecanisms()
    {
        Debug.Log("Activated");
        for (int i = 0; i < attached_mecanisms.Count; i++)
        {
            attached_mecanisms[i].Activate();
        }
    }

    private void OnTriggerEnter(Collider other)
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
        if (other.gameObject == activation_for
            && Vector3.Distance(transform.position, other.transform.position) > transform.localScale.x * 0.75f)
        {
            Debug.Log("Exit");
            activation_for = null;
            activated = false;
        }
    }
}