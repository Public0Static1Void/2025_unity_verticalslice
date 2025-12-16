using System.Collections.Generic;
using UnityEngine;

public class SC_Button_Hold : SC_Button
{
    public float activation_mass = 5;

    private bool activated = false;

    private GameObject activation_for = null;


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
            DeactivateMecanisms();
            activation_for = null;
            activated = false;
        }
    }
}