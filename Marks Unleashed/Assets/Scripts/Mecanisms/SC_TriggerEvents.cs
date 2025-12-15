using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(BoxCollider))]
public class SC_TriggerEvents : MonoBehaviour
{
    public UnityEvent event_entry;
    public UnityEvent event_exit;

    private void OnTriggerEnter(Collider other)
    {
        event_entry.Invoke();
    }
    private void OnTriggerExit(Collider other)
    {
        event_exit.Invoke();
    }
}