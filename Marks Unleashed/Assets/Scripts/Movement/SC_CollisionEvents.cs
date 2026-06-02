using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;

public class SC_CollisionEvents : MonoBehaviour
{
    public UnityEvent collision_event = new UnityEvent();

    public Vector3 pos_collided;
    public ParticleSystem collision_parts;

    private Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }
    public void SpawnParticles(ParticleSystem parts)
    {
        if (pos_collided != Vector3.zero)
            parts.transform.position = pos_collided;
        parts.Play();
    }

    private void OnTriggerEnter(Collider other)
    {
        CheckCollision(other.gameObject);
    }
    private void OnCollisionEnter(Collision other)
    {
        CheckCollision(other.gameObject);
    }

    private void CheckCollision(GameObject other)
    {
        if (other.tag != "Player" && rb.linearVelocity.magnitude > 0.25f)
        {
            collision_event?.Invoke();
            Vector3 dir = other.transform.position - transform.position;
            pos_collided = transform.position - dir.normalized * 2;
            SpawnParticles(collision_parts);
        }

    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(pos_collided, 0.5f);
    }
}