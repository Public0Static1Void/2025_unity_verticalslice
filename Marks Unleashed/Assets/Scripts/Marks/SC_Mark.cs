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
        coll.radius = 0.5f;

        enable_collision_activation = true;
    }

    private void OnDestroy()
    {
        if (transform.parent != null)
        {
            Destroy(transform.parent.gameObject);
            SC_MarksPlacer.Instance.RemoveMarkFromPlaced(gameObject.name);
        }
    }

    public virtual void Activate()
    {
        Debug.Log("Mark activated");
    }

    public void OnTriggerEnter(Collider other)
    {
        if (!enable_collision_activation) return;

        Rigidbody rb_other = other.GetComponent<Rigidbody>();
        if (rb_other != null)
        {
            Debug.Log(rb_other.linearVelocity.magnitude);
            if (rb_other.linearVelocity.magnitude >= 3)
                Activate();
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, 3);
    }
}