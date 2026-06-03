using UnityEngine;
using static UnityEngine.ParticleSystem;

public class SC_ZoneBurnable : MonoBehaviour
{
    public GameObject parts_burn;
    private bool _ignited = false;

    public Vector3 pos_fire;

    public void Ignite(Vector3 spawn_pos, float range, float strength, GameObject particles = null)
    {
        if (_ignited) return;

        _ignited = true;

        // Add a burn zone that will elevate burnable objects
        GameObject burnZone = new GameObject("BurnZone");
        burnZone.transform.position = transform.position + transform.up;
        burnZone.transform.rotation = transform.rotation;

        SC_ZoneBurn zone = burnZone.AddComponent<SC_ZoneBurn>();
        zone.particles = particles != null ? particles : parts_burn;
        zone.pos_particles = transform.position + pos_fire;
        zone.StartBurn(strength * 0.25f, range);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(pos_fire + transform.position, 0.1f);
    }
}
