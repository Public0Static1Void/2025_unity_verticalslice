using UnityEngine;

public class SC_Mark_Water : SC_Mark
{
    public float range = 3;

    public override void Activate()
    {
        base.Activate();
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
            if (coll.name == name) continue;

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

        DesactivateMark();
    }
}