using UnityEngine;

public class SC_Mark_Gravity : SC_Mark
{
    public float range = 15;

    public override void Activate()
    {
        base.Activate();
        GameObject black_hole = Instantiate(particles, transform.position, transform.rotation);
        black_hole.transform.Rotate(new Vector3(20, 0, 0));
        Destroy(black_hole, 4);

        Collider[] colls = Physics.OverlapSphere(transform.position, range, layer_interactable);
        foreach (Collider coll in colls)
        {
            if (coll.name == name) continue;

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

        DesactivateMark();
    }
    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, range);
    }

}