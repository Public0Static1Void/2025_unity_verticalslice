using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;

[RequireComponent(typeof(SphereCollider))]
public class SC_CollisionEvents : MonoBehaviour
{
    public UnityEvent collision_event = new UnityEvent();

    private void Start()
    {
        SphereCollider coll = GetComponent<SphereCollider>();
        coll.isTrigger = true;
        coll.radius = 1;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag != "Player")
            collision_event?.Invoke();
    }
}