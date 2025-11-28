using UnityEngine;
using System.Collections.Generic;


[RequireComponent(typeof(BoxCollider))]
public class SC_ZoneSlow : MonoBehaviour
{
    private Dictionary<string, SC_ObjectMovement> slowed_mov = new Dictionary<string, SC_ObjectMovement>();

    public float range = 3;

    private float slow_amount = 0.5f;

    void Start()
    {
        GetComponent<BoxCollider>().isTrigger = true;
    }

    private void Update()
    {
        range -= Time.deltaTime;
        if (range <= 0) return;

        Collider[] colls = Physics.OverlapSphere(transform.position, range);

        if (colls.Length == 0 && slowed_mov.Count > 0)
        {
            slowed_mov.Clear();
        }
        else if (colls.Length > 0)
        {
            foreach(Collider coll in colls)
            {
                if (!slowed_mov.ContainsKey(coll.name))
                {
                    slowed_mov.Add(coll.name, coll.GetComponent<SC_ObjectMovement>());
                }
            }
            foreach (KeyValuePair<string, SC_ObjectMovement> pair in slowed_mov)
            {
                SC_ObjectMovement mov = pair.Value;

                if (mov != null)
                    mov.SetSpeed(mov.max_speed * slow_amount);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        SC_ObjectMovement mov = other.GetComponent<SC_ObjectMovement>();
        if (mov != null)
        {
            mov.Slow(0.85f, 2);
        }
    }
}