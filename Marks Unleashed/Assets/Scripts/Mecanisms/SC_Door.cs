using UnityEngine;

public class SC_Door : SC_Interactable
{
    private MeshRenderer meshRenderer;
    private Collider coll;

    private void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        coll = GetComponent<Collider>();

        Deactivate();
    }
    public override void Activate()
    {
        Debug.Log("True");
        meshRenderer.enabled = false;
        coll.enabled = false;
    }
    public override void Deactivate()
    {
        Debug.Log("False");
        meshRenderer.enabled = true;
        coll.enabled = true;
    }
}