using UnityEngine;
using System.Collections.Generic;

public class SC_WindWheel : SC_Interactable
{
    public List<SC_Interactable> attached_mechanisms;
    [SerializeField] private float _rotSpeed = -10.0f;
    public bool while_active = false;

    private void Update()
    {
        if (while_active)
            RotateWheel();
    }
    public override void Activate()
    {
        while_active = true;
        for (int i = 0; i < attached_mechanisms.Count; i++)
        {
            attached_mechanisms[i].Activate();
        }
    }
    public override void Deactivate() 
    {
        while_active = false;
        for (int i = 0; i < attached_mechanisms.Count; i++)
        {
            attached_mechanisms[i].Deactivate();
        }
    }

    void RotateWheel()
    {
        transform.Rotate(0, 0, _rotSpeed * Time.deltaTime);
    }
}