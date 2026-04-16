using UnityEngine;
using static UnityEngine.ParticleSystem;

public class SC_ZoneBurnable : MonoBehaviour
{
    public GameObject parts_burn;
    private bool _ignited = false;

    public void Ignite(Vector3 spawn_pos, float range, float strength, GameObject particles = null)
    {
        if (_ignited) return;

        _ignited = true;

        // Add a burn zone that will elevate burnable objects
        GameObject burnZone = new GameObject("BurnZone");
        burnZone.transform.position = spawn_pos;
        burnZone.transform.rotation = transform.rotation;

        SC_ZoneBurn zone = burnZone.AddComponent<SC_ZoneBurn>();
        zone.particles = particles != null ? particles : parts_burn;
        zone.StartBurn(strength * 0.25f, range);
    }
}
