using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class SC_StartEvents : MonoBehaviour
{
    public UnityEvent start_event;
    void Start()
    {
        StartCoroutine(Delay());
    }
    private IEnumerator Delay()
    {
        yield return null;
        start_event?.Invoke();
    }
}