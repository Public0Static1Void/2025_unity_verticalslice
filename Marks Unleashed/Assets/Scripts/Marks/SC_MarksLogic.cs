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
        }

        ParticleSystem puddle_parts = water_puddle.GetComponent<ParticleSystem>();
        ParticleSystem.MainModule puddle_main = puddle_parts.main;
        
        LayerMask water_layer = 1 << LayerMask.NameToLayer("Water");
        colls = Physics.OverlapSphere(transform.position, range, water_layer);
        foreach (Collider coll in colls)
        {
            // Get the other water puddle particles
            ParticleSystem other_parts = coll.GetComponent<ParticleSystem>();

            ParticleSystem.MainModule other_main = other_parts.main;
            float lifetime = other_main.startLifetime.constant;

            puddle_parts.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            other_parts.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            other_main.startLifetime = lifetime + 4f;
            puddle_main.startLifetime = lifetime + 4;

            other_main.duration = lifetime + 4f;
            puddle_main.duration = lifetime + 4;

            puddle_parts.Play(true);
            other_parts.Play(true);
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
        }

        // Electrify nearby water
        LayerMask water_layer = 1 << LayerMask.NameToLayer("Water");
        colls = Physics.OverlapSphere(transform.position, range, water_layer);
        foreach (Collider coll in colls)
        {
            GameObject electric_zone = new GameObject("Electric zone");

            electric_zone.transform.SetParent(coll.transform, true);
            electric_zone.transform.position = coll.transform.position;

            SC_ZoneElectric zone = electric_zone.AddComponent<SC_ZoneElectric>();

            zone.puddle_system = coll.GetComponent<ParticleSystem>();
            zone.electric_shock = particles.GetComponent<ParticleSystem>();

            zone.Init();
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
    public float range = 3;

    public override void Activate()
    {
        Destroy(Instantiate(particles, transform.position, transform.rotation), 5);

        Collider[] colls = Physics.OverlapSphere(transform.position, range, layer_interactable);
        foreach (Collider coll in colls)
        {
            SC_ObjectMovement objectMovement = coll.GetComponent<SC_ObjectMovement>();
            if (objectMovement != null)
            {
                Vector3 dir = (coll.transform.position - transform.position).normalized;
                objectMovement.rb.AddForce(dir * element.strength * 250, ForceMode.Impulse);
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
    public float range = 10;

    public override void Activate()
    {
        Destroy(Instantiate(particles, transform.position, transform.rotation), 5);

        Collider[] colls = Physics.OverlapSphere(transform.position, range, layer_interactable);
        foreach (Collider coll in colls)
        {
            SC_ObjectMovement objectMovement = coll.GetComponent<SC_ObjectMovement>();
            if (objectMovement != null)
            {
                Vector3 dir = (transform.position - coll.transform.position).normalized;
                objectMovement.rb.AddForce(dir * element.strength * 400, ForceMode.Impulse);
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