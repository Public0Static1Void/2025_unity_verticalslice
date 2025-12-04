using UnityEngine;
using UnityEngine.UI;

public class SC_Mark : MonoBehaviour
{
    public Element element;
    public GameObject particles;

    public LayerMask layer_interactable = ~0;

    public bool enable_collision_activation = false;

    public void Init(Element el, GameObject parts)
    {
        element = el;
        particles = parts;
    }
    public void EnableCollision()
    {
        SphereCollider coll = gameObject.AddComponent<SphereCollider>();
        coll.isTrigger = true;
        coll.radius = 1;

        enable_collision_activation = true;
    }

    private void OnDestroy()
    {
        if (transform.parent != null)
        {
            Destroy(transform.parent.gameObject);
        }
    }

    public virtual void Activate()
    {
        Debug.Log("Mark activated");
    }

    public void OnTriggerEnter(Collider other)
    {
        if (!enable_collision_activation) return;

        Rigidbody rb = other.GetComponent<Rigidbody>();
        if (rb != null && rb.linearVelocity.magnitude > 5)
        {
            Activate();
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, 3);
    }
}