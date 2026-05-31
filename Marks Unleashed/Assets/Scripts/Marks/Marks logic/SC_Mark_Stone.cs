using UnityEngine;
using static UnityEngine.UI.Image;

public class SC_Mark_Stone : SC_Mark
{
    public float range = 3;

    private bool _activated = false;
    public override void ActivateMark()
    {
        _activated = true;

        base.ActivateMark();
        //Instancia partículas y las destruye desués de X segundos
        int layer = ~LayerMask.GetMask("Player", "Physic Object");
        float dist = 9;
        if (Physics.Raycast(transform.position + transform.forward, transform.up, out RaycastHit hit, 9, layer))
        {
            Debug.Log($"Hit {hit.distance}");
            dist = hit.distance;
        }
        GameObject stone_wall = Instantiate(particles, transform.position + transform.up * (9 - (9 - dist)), transform.rotation);
        stone_wall.GetComponent<SC_Test_Mark_Stone>().sc_Mark_Stone_Visualizer = GetComponentInChildren<SC_Mark_Stone_Visualizer>();
        Destroy(stone_wall, 5);

        DesactivateMark();
    }

    private void Update()
    {
        if (_activated) return;

        Collider[] colls = Physics.OverlapSphere(transform.position, range, layer_interactable);
        if (colls.Length > 0)
        {
            foreach (Collider coll in colls)
            {
                if (coll.name == name) continue;

                Vector3 origin = transform.position + transform.up * (coll.transform.localScale.magnitude / 2 < 1 ? 1 : coll.transform.localScale.magnitude / 2);
                Rigidbody rb = coll.GetComponent<Rigidbody>();
                if (rb != null && rb.linearVelocity.magnitude < 1)
                {
                    Vector3 pos = new Vector3(rb.position.x, transform.position.y, rb.position.z);
                    Vector3 diff = origin - pos;
                    if (diff.magnitude < 0.05f) continue;

                    Vector3 dir = diff.normalized;
                    rb.linearVelocity =  dir;
                }
            }
        }
    }
}