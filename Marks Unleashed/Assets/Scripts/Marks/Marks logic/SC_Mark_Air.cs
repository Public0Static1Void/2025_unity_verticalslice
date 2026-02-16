using UnityEngine;

public class SC_Mark_Air : SC_Mark
{
    public float range = 4;

    public override void Activate()
    {
        base.Activate();
        Destroy(Instantiate(particles, transform.position, Quaternion.LookRotation(transform.up, transform.forward)), 5);

        Collider[] colls = Physics.OverlapSphere(transform.position, range, layer_interactable);
        foreach (Collider coll in colls)
        {
            if (coll.name == name) continue;

            Rigidbody rb = coll.GetComponent<Rigidbody>();
            if (rb != null && !rb.isKinematic)
            {
                rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
                rb.AddForce(transform.forward * GetElementStrenght() * 150, ForceMode.Impulse);
            }
            SC_LifeSystem life = coll.GetComponent<SC_LifeSystem>();
            if (life != null)
            {
                life.ApplyElement(element);
            }
        }

        DesactivateMark();
    }
}