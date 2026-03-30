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

    public void InitAirZone(float new_range, float new_horizontal_range, LayerMask layerMask, float new_force, GameObject new_particles, bool can_destroy = false)
    {
        base.InitZone(new_range, new_horizontal_range, new_particles, can_destroy);
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
        string[] names_inside = new string[0];
        Collider[] colls = Physics.OverlapSphere(transform.position, range, layer_interactable);
        if (colls.Length > 0)
        {
            names_inside = new string[colls.Length];
            for (int i = 0; i < colls.Length; i++)
            {
                names_inside[i] = colls[i].name;

                // Check if there is a direct line of vision between the object and the player
                Vector3 origin = transform.position;
                Vector3 closest_point = colls[i].ClosestPoint(transform.position);
                Vector3 direction = (colls[i].transform.position - origin).normalized;
                float distance = Vector3.Distance(origin, colls[i].transform.position);

                float distance_from_center = Vector3.Distance(origin, new Vector3(closest_point.x, transform.position.y, closest_point.z));
                if (distance_from_center > range_horizontal)
                {
                    RemoveName(colls[i].name);
                    continue;
                }

                if (Physics.Raycast(origin, direction, out RaycastHit hit, distance))
                {
                    if (hit.collider != colls[i])
                    {
                        RemoveName(colls[i].name);
                        continue;
                    }
                    
                    Rigidbody rb = hit.collider.GetComponent<Rigidbody>();
                    if (rb != null && !affected_names.Contains(colls[i].name))
                    {
                        affected_names.Add(colls[i].name);
                        affected_rb.Add(rb);
                    }

                    Debug.DrawLine(origin, hit.point, Color.green);
                }
                else
                {
                    RemoveName(colls[i].name);

                    Debug.DrawLine(origin, origin + direction * distance, Color.red);
                    continue;
                }
            }

            List<int> remove_indexs = new List<int>();
            for (int i = 0; i < affected_names.Count; i++)
            {
                bool name_inside = false;
                for (int j = 0; j < names_inside.Length; j++)
                {
                    if (affected_names[i] == names_inside[j])
                        name_inside = true;
                }
                if (!name_inside)
                {
                    remove_indexs.Add(i);
                }
            }

            for (int i = 0;i < remove_indexs.Count; i++)
            {
                affected_names.RemoveAt(remove_indexs[i]);
                affected_rb.RemoveAt(remove_indexs[i]);
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

    private void RemoveName(string name)
    {
        if (affected_names.Contains(name))
        {
            int indx = affected_names.IndexOf(name);
            if (indx != -1)
            {
                affected_names.RemoveAt(indx);
                affected_rb.RemoveAt(indx);
            }
        }
    }
}