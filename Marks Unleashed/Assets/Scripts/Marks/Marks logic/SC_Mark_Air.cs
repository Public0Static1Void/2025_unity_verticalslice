using UnityEngine;

public class SC_Mark_Air : SC_Mark
{
    public float range = 10;
    public float initial_strength = 100, strength = 100;

    public override void ActivateMark()
    {
        base.ActivateMark();
        particles = Instantiate(particles, transform.position, Quaternion.LookRotation(transform.up, transform.forward));

        Collider[] colls = Physics.OverlapSphere(transform.position, range, layer_interactable);
        foreach (Collider coll in colls)
        {
            if (coll.name == name) continue;

            if (coll.name == "Player")
            {
                SC_CameraRotation.instance.ShakeCamera(0.75f, 0.2f);
            }

            Rigidbody rb = coll.GetComponent<Rigidbody>();
            if (rb != null && !rb.isKinematic)
            {
                rb.AddForce(transform.forward * GetElementStrength() * initial_strength, ForceMode.Impulse);
            }
        }

        // Creates an air zone
        GameObject ob_airZone = new GameObject("Air zone");
        ob_airZone.transform.position = transform.position;
        ob_airZone.transform.rotation = transform.rotation;
        if (transform.parent != null && transform.parent.parent != null)
        {
            ob_airZone.transform.SetParent(transform.parent.parent, true);
        }

        SC_ZoneAir zone_air = ob_airZone.AddComponent<SC_ZoneAir>();
        zone_air.InitAirZone(range, range / 2, layer_interactable, GetElementStrength() * strength, particles, destroyable);


        DesactivateMark();
    }
}