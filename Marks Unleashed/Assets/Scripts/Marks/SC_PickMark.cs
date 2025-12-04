using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(BoxCollider))]
public class SC_PickMark : SC_Interactable
{
    public ElementTypes elementType;

    public override void Activate()
    {
        SC_MarksPlacer.Instance.UnlockMark(elementType);
        Destroy(gameObject);
    }
}