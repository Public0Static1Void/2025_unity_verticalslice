using UnityEngine;
using System.Collections;
using UnityEngine.Rendering;

public class Mark_Water : SC_Mark
{
    public float range = 3;

    public override void Activate()
    {
        GameObject ob = Instantiate(particles, transform.position, transform.rotation);
        // Get the water puddle child
        Transform water_puddle = ob.transform.GetChild(3);
        water_puddle.SetParent(null, true);
        // Destroy the parent in 5 seconds
        Destroy(ob, 5);

        // Adds a slow zone
        GameObject slow_zone = new GameObject("Slow zone");

        SC_ZoneSlow zone = slow_zone.AddComponent<SC_ZoneSlow>();
        zone.range = range;
            /// Sets it's parent to the water puddle
        slow_zone.transform.SetParent(water_puddle, true);
        slow_zone.transform.position = transform.position;


        LayerMask water_layer = 1 << LayerMask.NameToLayer("Water");

        ParticleSystem puddle_parts = water_puddle.GetComponent<ParticleSystem>();
        ParticleSystem.MainModule puddle_main = puddle_parts.main;


        Collider[] colls = Physics.OverlapSphere(transform.position, range, layer_interactable);
        foreach (Collider coll in colls)
        {
            SC_LifeSystem lifeSystem = coll.GetComponent<SC_LifeSystem>();
            if (lifeSystem != null)
            {
                lifeSystem.ApplyElement(SC_ElementManager.instance.elements[(int)ElementTypes.WATER]);
            }

            SC_ObjectMovement objectMovement = coll.GetComponent<SC_ObjectMovement>();
            if (objectMovement != null)
            {
                objectMovement.Slow(1 - element.strength * 0.25f, element.strength * 1.5f);
            }

            // Nearby water detection
            if (SC_GameManager.ObjectContainsLayer(coll.gameObject, water_layer))
            {
                // Get the other water puddle particles
                ParticleSystem other_parts = coll.GetComponent<ParticleSystem>();

                ParticleSystem.MainModule other_main = other_parts.main;
                float lifetime = other_main.startLifetime.constant;

                puddle_parts.Stop(true, ParticleSystemStopBehavior.StopEmitting);
                other_parts.Stop(true, ParticleSystemStopBehavior.StopEmitting);

                other_main.startLifetime = lifetime + 4f;
                puddle_main.startLifetime = lifetime + 4;

                puddle_parts.Play(true);
                other_parts.Play(true);
            }
        }

        Destroy(gameObject);
    }
}
public class Mark_Electricity : SC_Mark
{
    public float range = 3;

    public override void Activate()
    {
        Destroy(Instantiate(particles, transform.position + transform.up * 2, transform.rotation), 5);

        LayerMask water_layer = 1 << LayerMask.NameToLayer("Water");

        Collider[] colls = Physics.OverlapSphere(transform.position, range, layer_interactable);
        foreach (Collider coll in colls)
        {
            SC_LifeSystem lifeSystem = coll.GetComponent<SC_LifeSystem>();
            if (lifeSystem != null)
            {
                lifeSystem.ApplyElement(SC_ElementManager.instance.elements[(int)ElementTypes.ELECTRICITY]);
            }

            SC_ObjectMovement movementScript = coll.GetComponent<SC_ObjectMovement>();
            if (movementScript != null)
            {
                movementScript.StopMovement(2);
            }

            // Electrify nearby water
            if (SC_GameManager.ObjectContainsLayer(coll.gameObject, water_layer))
            {
                GameObject electric_zone = new GameObject("Electric zone");

                electric_zone.transform.SetParent(coll.transform, true);
                electric_zone.transform.localPosition = Vector3.zero;

                SC_ZoneElectric zone = electric_zone.AddComponent<SC_ZoneElectric>();

                zone.puddle_system = coll.GetComponent<ParticleSystem>();
                zone.electric_shock = particles.GetComponent<ParticleSystem>();

                zone.Init();
            }
        }

        Destroy(gameObject);
    }
}
public class Mark_Fire : SC_Mark
{
    public float range = 3;

    public override void Activate()
    {
        Destroy(Instantiate(particles, transform.position, transform.rotation), 5);

        Collider[] colls = Physics.OverlapSphere(transform.position, range, layer_interactable);
        foreach (Collider coll in colls)
        {
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

        Destroy(gameObject);
    }
}
public class Mark_Stone : SC_Mark
{
    public float range = 3;

    public override void Activate()
    {
        Destroy(Instantiate(particles, transform.position, transform.rotation), 5);

        Destroy(gameObject);
    }
}
public class Mark_Air : SC_Mark
{
    public float range = 4;

    public override void Activate()
    {
        Destroy(Instantiate(particles, transform.position, Quaternion.LookRotation(transform.up, transform.forward)), 5);

        Collider[] colls = Physics.OverlapSphere(transform.position, range, layer_interactable);
        foreach (Collider coll in colls)
        {
            Rigidbody rb = coll.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.AddForce(transform.forward * element.strength * 150, ForceMode.Impulse);
            }
            SC_LifeSystem life = coll.GetComponent<SC_LifeSystem>();
            if (life != null)
            {
                life.ApplyElement(element);
            }
        }

        Destroy(gameObject);
    }
}
public class Mark_Gravity : SC_Mark
{
    public float range = 15;

    public override void Activate()
    {
        GameObject black_hole = Instantiate(particles, transform.position, transform.rotation);
        black_hole.transform.Rotate(new Vector3(20, 0, 0));
        Destroy(black_hole, 5);

        Collider[] colls = Physics.OverlapSphere(transform.position, range, layer_interactable);
        foreach (Collider coll in colls)
        {
            // Pulls all nearby objects
            Rigidbody rb = coll.GetComponent<Rigidbody>();
            if (rb != null)
            {
                // Calculates the pull force based on the distance
                float dist = Vector3.Distance(transform.position, rb.position);
                Debug.Log((0.1f + 1 - (dist / range)));
                // Adds force in the impulse direction
                Vector3 dir = (transform.position - coll.transform.position).normalized * (0.1f + 1 - (dist / range));
                rb.AddForce(dir * element.strength * 400, ForceMode.Impulse);
            }
            SC_LifeSystem life = coll.GetComponent<SC_LifeSystem>();
            if (life != null)
            {
                life.ApplyElement(element);
            }
        }

        Destroy(gameObject);
    }
    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, range);
    }

}