using UnityEngine;

public class SC_Mark_Electricity : SC_Mark
{
    public float range = 3;

    public override void Activate()
    {
        base.Activate();
        Destroy(Instantiate(particles, transform.position + transform.up * 2, transform.rotation), 5);

        LayerMask water_layer = 1 << LayerMask.NameToLayer("Water");

        Collider[] colls = Physics.OverlapSphere(transform.position, range, layer_interactable);
        foreach (Collider coll in colls)
        {
            if (coll.name == name) continue;

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

        DesactivateMark();
    }
}