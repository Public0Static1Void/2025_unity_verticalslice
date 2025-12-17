using System.Collections.Generic;
using UnityEngine;

public class SC_Button_Hold : SC_Button
{
    public float activation_mass = 5;

    private bool activated = false;

    private GameObject activation_for = null;

    private float activate_timer = 0;
    private const float activation_cooldown = 1;

    private bool can_activate = true;

    public float detection_range = 1;

    private void Update()
    {
        if (!can_activate)
        {
            activate_timer += Time.deltaTime;
            if (activate_timer >= activation_cooldown)
            {
                activate_timer = 0;
                can_activate = true;
            }
        }
    }

    private void FixedUpdate()
    {
        Collider[] colls = Physics.OverlapSphere(transform.position, detection_range);

        if (colls.Length == 0)
        {
            if (activation_for != null)
            {
                DeactivateMecanisms();
                activation_for = null;
                activated = false;
            }
            return;
        }

        if (activation_for != null)
        {
            // Check if the same gameObject is still inside
            foreach (Collider coll in colls)
            {
                if (coll.gameObject == activation_for)
                    return;
            }

            // The object left the range
            DeactivateMecanisms();
            activation_for = null;
            activated = false;
            return;
        }

        // Find nearest valid object
        Collider nearest = null;
        float nearestDist = float.MaxValue;

        foreach (Collider coll in colls)
        {
            if (coll.name == name) continue;

            // mass check
            Rigidbody rb = coll.attachedRigidbody;
            if (rb == null || rb.mass < activation_mass) continue;

            float dist = Vector3.Distance(transform.position, coll.transform.position);
            if (dist < nearestDist)
            {
                nearest = coll;
                nearestDist = dist;
            }
        }

        // Activate once
        if (nearest != null)
        {
            ActivateMecanisms();
            activation_for = nearest.gameObject;
            activated = true;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, detection_range);
    }
}