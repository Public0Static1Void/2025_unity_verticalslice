using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(BoxCollider))]
public class SC_TriggerEvents : MonoBehaviour
{
    public UnityEvent event_entry;
    public UnityEvent event_exit;

    private bool active = true;

    private void OnTriggerEnter(Collider other)
    {
        if (!active) return;

        event_entry.Invoke();
    }
    private void OnTriggerExit(Collider other)
    {
        event_exit.Invoke();
    }

    public void Deabilitate()
    {
        active = false;
    }
}