using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;

public class SC_CollisionEvents : MonoBehaviour
{
    public UnityEvent collision_event = new UnityEvent();

    public Vector3 pos_collided;

    public void SpawnParticles(GameObject parts)
    {
        GameObject ob = Instantiate(parts, transform.position, parts.transform.rotation);

        if (pos_collided != Vector3.zero)
            ob.transform.position = pos_collided;

        Destroy(ob, 5);
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
        if (other.tag != "Player")
        {
            collision_event?.Invoke();
            Vector3 dir = other.transform.position - transform.position;
            pos_collided = transform.position + dir.normalized * 2;
        }

    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(pos_collided, 0.5f);
    }
}