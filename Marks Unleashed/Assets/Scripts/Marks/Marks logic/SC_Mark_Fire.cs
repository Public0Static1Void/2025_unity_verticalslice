using UnityEngine;

public class SC_Mark_Fire : SC_Mark
{
    public float range = 6;

    public override void ActivateMark()
    {
        base.ActivateMark();

        GameObject parts_parent = Instantiate(particles, transform.position, transform.rotation);
        ParticleSystem parts = parts_parent.transform.GetChild(0).GetComponentInChildren<ParticleSystem>();
        ParticleSystem.MainModule main = parts.main;
        parts.Stop();
        main.duration = 0.1f;
        parts.Play();

        Destroy(parts.gameObject, 5);

        Collider[] colls = Physics.OverlapSphere(transform.position, range, layer_interactable);
        foreach (Collider coll in colls)
        {
            if (coll.name == name) continue;

            SC_LifeSystem lf = coll.GetComponent<SC_LifeSystem>();
            if (lf != null)
            {
                lf.BurnDamage(element.strength, element.strength * 2);
            }
        }

        LayerMask burnable_layer = 1 << LayerMask.NameToLayer("Burnable");
        colls = Physics.OverlapSphere(transform.position, range, burnable_layer);
        foreach (Collider coll in colls)
        {
            // Add a burn zone that will elevate burnable objects
            GameObject burnZone = new GameObject("BurnZone");
            burnZone.transform.position = transform.position;
            burnZone.transform.rotation = Quaternion.LookRotation(transform.up);

            SC_ZoneBurn zone = burnZone.AddComponent<SC_ZoneBurn>();
            zone.particles = particles;
            zone.StartBurn(element.strength, range);            
        }

        DesactivateMark();
    }
}