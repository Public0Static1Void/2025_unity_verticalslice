using UnityEngine;
using System.Collections.Generic;

public class SC_ZoneAir : SC_Zone
{
    public float force = 1;

    public LayerMask layer_interactable;

    private List<Rigidbody> affected_rb;
    private List<string> affected_names;

    protected override void Start()
    {
        base.Start();
        affected_rb = new List<Rigidbody>();
        affected_names = new List<string>();
    }



    public void InitAirZone(float new_range, LayerMask layerMask, float new_force, GameObject new_particles)
    {
        base.InitZone(new_range, new_particles);
        layer_interactable = layerMask;
        force = new_force;
    }

    private void Update()
    {
        CheckForNewObjects();
        ApplyForces();
    }

    private void CheckForNewObjects()
    {
        // Search for new objects
        string[] names_on_range = new string[0];

        Collider[] colls = Physics.OverlapSphere(transform.position, range, layer_interactable);
        if (colls.Length > 0)
        {
            names_on_range = new string[colls.Length];
            for (int i = 0; i < colls.Length; i++)
            {
                names_on_range[i] = colls[i].name;

                // Don't check twice the same rigidbody or don't affect it's parent
                if (affected_names.Contains(colls[i].name) || colls[i].name == transform.parent.name) continue;

                // Save the rigidbody
                Rigidbody rb = colls[i].GetComponent<Rigidbody>();
                if (rb != null)
                {
                    // Adds a small start force
                    //rb.AddForce(transform.forward * (force * 0.25f), ForceMode.Impulse);

                    affected_rb.Add(rb);
                    affected_names.Add(colls[i].name);
                }
            }

            
        }
        /// Removes the names that aren't on the range
        for (int i = 0; i < affected_names.Count; i++)
        {
            bool name_on_list = false;
            for (int j = 0; j < names_on_range.Length; j++)
            {
                if (affected_names[i] == names_on_range[j])
                {
                    name_on_list = true;
                    break;
                }
            }
            if (!name_on_list)
            {
                affected_names.RemoveAt(i);
                affected_rb.RemoveAt(i);
            }
        }
    }
    
    private void ApplyForces()
    {
        if (affected_rb.Count <= 0) return;
        for (int i = 0; i < affected_rb.Count; i++)
        {
            if (affected_rb[i] == null)
            {
                continue;
            }

            float targetHeight = range;
            float currentHeight = affected_rb[i].transform.position.y - transform.position.y;

            float heightError = targetHeight - currentHeight; // Height difference

            float floatForce = heightError * force; // Spring-like movement
            float damping = -affected_rb[i].linearVelocity.y * (force * 0.1f); // Damps the velocity to prevent bouncing

            affected_rb[i].AddForce(transform.forward * (floatForce + damping), ForceMode.Force);
        }
    }
}