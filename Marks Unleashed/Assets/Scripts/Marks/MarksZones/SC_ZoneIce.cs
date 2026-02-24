using UnityEngine;

public class SC_ZoneIce : SC_Zone
{
    private LayerMask layer_movable;

    protected override void Start()
    {
        base.Start();

        layer_movable = LayerMask.NameToLayer("Grabbable") + LayerMask.NameToLayer("Physic Object");
    }
    private void Update()
    {
        
    }
}