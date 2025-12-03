using UnityEngine;

public class SC_Door : SC_Interactable
{
    private MeshRenderer meshRenderer;

    private void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();

        Deactivate();
    }
    public override void Activate()
    {
        Debug.Log("True");
        meshRenderer.enabled = false;
    }
    public override void Deactivate()
    {
        Debug.Log("False");
        meshRenderer.enabled = true;
    }
}