using UnityEngine;

public abstract class SC_Zone : MonoBehaviour
{
    [Header("Stats")]
    public float range = 3;
    protected SphereCollider zone_collider;

    [Header("References")]
    public GameObject particles;

    protected virtual void Start()
    {
        zone_collider = gameObject.AddComponent<SphereCollider>();
        zone_collider.isTrigger = true;
        zone_collider.radius = range;

        gameObject.layer = LayerMask.NameToLayer("Mark");
    }

    public virtual void InitZone(float new_range, GameObject new_particles)
    {
        range = new_range;
        particles = new_particles;
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
}