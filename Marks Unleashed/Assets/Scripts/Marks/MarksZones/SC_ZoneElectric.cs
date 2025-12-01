using UnityEngine;
using System.Collections.Generic;
using System.Collections;

[RequireComponent(typeof(BoxCollider))]
public class SC_ZoneElectric : MonoBehaviour
{
    public int damage = 1;
    public float stunDuration = 2;

    public ParticleSystem electric_shock;

    public ParticleSystem puddle_system;
    private ParticleSystem.Particle puddle_particle;
    private ParticleSystem.Particle[] puddle_particles;

    public void Init()
    {
        if (transform.parent == null)
        {
            Debug.LogWarning("Electric zone created without parent");
            Destroy(this);
        }

        ParticleSystem parts = Instantiate(electric_shock, transform.position, transform.rotation);

        parts.transform.SetParent(transform, true);

        ParticleSystem.MainModule parts_main = parts.main;
        parts_main.loop = true;
        parts_main.startLifetime = 0.05f;

        GetComponent<BoxCollider>().isTrigger = true;

        transform.localScale = Vector3.one * 3;

        puddle_particles = new ParticleSystem.Particle[puddle_system.main.maxParticles];
        if (puddle_system.particleCount > 0)
        {
            puddle_particle = puddle_particles[0];
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Entered");
        SC_ObjectMovement mov = other.GetComponent<SC_ObjectMovement>();
        if (mov != null)
        {
            other.GetComponent<SC_LifeSystem>().Damage(damage);
            mov.StopMovement(stunDuration);
        }
    }
}
