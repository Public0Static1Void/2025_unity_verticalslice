using UnityEngine;
using System.Collections.Generic;

public class SC_ZoneAir : SC_Zone
{
    public float force = 1;

    public void InitAirZone(float new_range, float new_horizontal_range, LayerMask layerMask, float new_force, GameObject new_particles, bool can_destroy = false)
    {
        base.InitZone(new_range, new_horizontal_range, new_particles, can_destroy);
        layer_interactable = layerMask;
        force = new_force;

        if (range > 15)
        {
            ParticleSystem[] pss = GetComponentsInChildren<ParticleSystem>();
            foreach (ParticleSystem ps in pss)
            {
                if (ps.name.Contains("Cloud")) continue;

                ParticleSystem.MainModule main = ps.main;
                main.startLifetime = range / 4;
                ParticleSystem.ShapeModule shape = ps.shape;
                shape.radius = range / 2;
                ParticleSystem.VelocityOverLifetimeModule vel = ps.velocityOverLifetime;
                vel.orbitalZ = Mathf.Min(1, range / 5);
            }
        }

        SphereCollider coll = gameObject.AddComponent<SphereCollider>();
        coll.radius = range;
        coll.isTrigger = true;
    }

    private void Update()
    {
        CheckForNewObjects();
        ApplyForces();
    }

    private void ApplyForces()
    {
        if (affected_rb.Count <= 0) return;
        for (int i = 0; i < affected_rb.Count; i++)
        {
            if (affected_rb[i] == null || transform.parent != null && affected_rb[i].name == transform.parent.name)
            {
                continue;
            }

            float targetHeight = range;
            float currentHeight = affected_rb[i].transform.position.y - transform.position.y;

            float heightError = targetHeight - currentHeight; // Height difference

            float floatForce = heightError * force; // Spring-like movement
            float damping = -affected_rb[i].linearVelocity.y * (force * 0.75f); // Damps the velocity to prevent bouncing

            ForceMode forceMode = ForceMode.Force;
            if (affected_rb[i].CompareTag("MovableCube"))
            {
                forceMode = ForceMode.Force;
                floatForce *= affected_rb[i].mass;
            }

            affected_rb[i].AddForce(transform.forward * (floatForce + damping), forceMode);
        }
    }

    public void Pause()
    {
        ParticleSystem[] pss = GetComponentsInChildren<ParticleSystem>();
        foreach (ParticleSystem ps in pss)
        {
            ps.Pause();
        }
    }
    public void Resume()
    {
        ParticleSystem[] pss = GetComponentsInChildren<ParticleSystem>();
        foreach (ParticleSystem ps in pss)
        {
            ps.Play();
        }
    }
}