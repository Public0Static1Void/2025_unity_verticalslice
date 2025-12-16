using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(BoxCollider))]
public class SC_PickMark : SC_Interactable
{
    public ElementTypes elementType;

    public override void Activate()
    {
        SC_MarksPlacer.Instance.UnlockMark(elementType);

        if (!SC_GameManager.instance.mark_unlocked)
        {
            SC_GameManager.instance.ShowText("Press Q to open the mark menu");
            SC_GameManager.instance.mark_unlocked = true;
        }

        Destroy(gameObject);
    }
}