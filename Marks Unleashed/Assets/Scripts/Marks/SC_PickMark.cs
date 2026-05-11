using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(BoxCollider))]
public class SC_PickMark : SC_Interactable
{
    public ElementTypes elementType;

    public UnityEvent events_on_pick;

    private string text = "Open_Rune_Menu";

    private void Start()
    {
        text = SC_LanguageManager.instance.GetText(text);
    }

    public override void Activate()
    {
        SC_MarksPlacer.Instance.UnlockMark(elementType);

        if (!SC_GameManager.instance.mark_unlocked)
        {
            SC_GameManager.instance.ShowText(text);
            SC_GameManager.instance.mark_unlocked = true;
        }

        events_on_pick?.Invoke();

        Destroy(gameObject);
    }
}