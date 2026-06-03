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

            if (Vector3.Distance(transform.position, coll.transform.position) < range / 2)
            {
                SC_LifeSystem lf = coll.GetComponent<SC_LifeSystem>();
                if (lf != null && element != null)
                {
                    lf.BurnDamage(element.strength * 0.25f, element.strength * 2);
                }
            }

            if (coll.CompareTag("Arrow"))
            {
                SC_Arrow arrow = coll.GetComponent<SC_Arrow>();
                if (arrow != null)
                    arrow.SetElement(ElementTypes.FIRE);
            }
        }

        LayerMask burnable_layer = 1 << LayerMask.NameToLayer("Combustible");
        colls = Physics.OverlapSphere(transform.position, range, burnable_layer);
        foreach (Collider coll in colls)
        {
            SC_ZoneBurnable zb = coll.GetComponent<SC_ZoneBurnable>();
            if (zb != null && element != null)
                zb.Ignite(coll.transform.position + coll.transform.up, range, element.strength);
        }

        DesactivateMark();
    }
}