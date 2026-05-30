using System.Collections.Generic;
using UnityEngine;

public class SC_Button_One : SC_Button, IN_Interactable
{
    private bool activated;
    public bool only_activate = false;

    public void Activate()
    {
        activated = only_activate ? true : !activated;

        if (activated)
            ActivateMecanisms();
        else
            DeactivateMecanisms();

        audioSource.Play();
    }

    public void Deactivate()
    {
        activated = false;
        DeactivateMecanisms();
    }
}