using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(BoxCollider))]
public class SC_TriggerEvents : MonoBehaviour
{
    public bool hasBeenTriggered = false;
    public string tag = "";
    public UnityEvent event_entry;
    public UnityEvent event_exit;

    private bool active = true;

    private void OnTriggerEnter(Collider other)
    {
        if (!active || hasBeenTriggered == true || (tag != "" && !other.CompareTag(tag))) return;

        event_entry.Invoke();
        hasBeenTriggered = true;
    }
    private void OnTriggerExit(Collider other)
    {
        if (!active || (tag != "" && !other.CompareTag(tag))) return;
        event_exit.Invoke();
    }

    public void Deabilitate()
    {
        active = false;
    }
}