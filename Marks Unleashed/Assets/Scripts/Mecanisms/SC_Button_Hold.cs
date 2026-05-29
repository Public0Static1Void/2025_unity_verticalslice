using System.Collections.Generic;
using UnityEngine;

public class SC_Button_Hold : SC_Button
{
    public float activation_mass = 5;

    private bool activated = false;

    public GameObject activation_for = null;

    private float activate_timer = 0;
    private const float activation_cooldown = 1;

    private bool can_activate = true;

    public LayerMask layer_interact;
    public float detection_range = 1;
    public float pull_force = 0.65f;
    public float deactivate_delay = 0;
    private bool _deactivate = false;
    private float _timer = 0;

    public bool active = true;

    private void Update()
    {
        if (!activated) return;

        if (!can_activate)
        {
            activate_timer += Time.deltaTime;
            if (activate_timer >= activation_cooldown)
            {
                activate_timer = 0;
                can_activate = true;
            }
        }

        if (activation_for != null && activation_for.name != "Player")
        {
            activation_for.transform.position = Vector3.Lerp(activation_for.transform.position, transform.position + transform.up * 0.75f, Time.deltaTime * pull_force);
        }
    }

    private void FixedUpdate()
    {
        if (!active) return;

        if (!_deactivate)
        {
            if (activated && activation_for == null)
            {
                _deactivate = true;
                activated = false;
                return;
            }

            Collider[] colls = Physics.OverlapSphere(transform.position, detection_range, layer_interact);

            if (colls.Length == 0)
            {
                if (activation_for != null)
                {
                    _deactivate = true;
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
                _deactivate = true;
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
        else
        {
            _timer += Time.deltaTime;
            if (_timer > deactivate_delay)
            {
                _timer = 0;
                _deactivate = false;
                DeactivateMecanisms();
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, detection_range);
        Gizmos.DrawWireSphere(transform.position + transform.up * 0.75f, 0.5f);
    }
}