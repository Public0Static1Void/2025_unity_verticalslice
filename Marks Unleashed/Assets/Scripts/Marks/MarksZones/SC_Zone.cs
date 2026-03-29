using UnityEngine;

public abstract class SC_Zone : MonoBehaviour
{
    [Header("Stats")]
    public float range = 3;
    public float range_horizontal = 3;
    protected SphereCollider zone_collider;
    public bool destroyable = false;

    [Header("References")]
    public GameObject particles;

    protected virtual void Start()
    {
        zone_collider = gameObject.AddComponent<SphereCollider>();
        zone_collider.isTrigger = true;
        zone_collider.radius = range;

        gameObject.layer = LayerMask.NameToLayer("Mark");
    }

    public virtual void InitZone(float new_range, float new_horizontal_range, GameObject new_particles, bool can_destroy = false)
    {
        range = new_range;
        range_horizontal = new_horizontal_range;
        particles = new_particles;
        destroyable = can_destroy;
        particles.transform.SetParent(transform, true);
    }
    protected virtual void OnDestroy()
    {
        if (particles == null) return;
        particles.transform.SetParent(null, true);
        particles.GetComponent<ParticleSystem>().Stop();
        Transform tr = particles.transform.GetChild(0);
        if (tr != null)
        {
            tr.GetComponent<ParticleSystem>().Stop();
        }
        Destroy(particles, 10);
        Debug.Log($"Destroyed: {name}");
    }

    public void HighlightZone()
    {
        Debug.Log($"Looking at: {name}");
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.gray;
        Gizmos.DrawWireSphere(transform.position, range);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, range_horizontal);
    }
}