using UnityEngine;

public class Mark_Fire : SC_Mark
{
    public float range = 3;

    public override void Activate()
    {
        base.Activate();
        Destroy(Instantiate(particles, transform.position, transform.rotation), 5);

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
            GameObject burnZone = new GameObject("BurnZone");
            SC_ZoneBurn zone = burnZone.AddComponent<SC_ZoneBurn>();
            zone.StartBurn(element.strength, coll.transform.localScale.x);
            Destroy(burnZone, 5);
        }

        DesactivateMark();
    }
}