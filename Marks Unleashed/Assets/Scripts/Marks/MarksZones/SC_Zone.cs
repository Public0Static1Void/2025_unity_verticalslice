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
    }
    protected virtual void OnDestroy()
    {
        Destroy(particles);
        Debug.Log($"Destroyed: {name}");
    }

    public void HighlightZone()
    {
        Debug.Log($"Looking at: {name}");
    }
}