using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SC_ZoneBurn : SC_Zone
{
    // Stats
    public float max_life = 3;
    private float current_life = 3;    


    private List<SC_Interactable> affected_objects = new List<SC_Interactable>();

    private LayerMask balloon_mask, combustible_mask;

    private Collider[] _colls;

    public Vector3 pos_particles = Vector3.zero;



    protected override void Start()
    {
        base.Start();

        current_life = max_life;

        balloon_mask = 1 << LayerMask.NameToLayer("Burnable");
        combustible_mask = 1 << LayerMask.NameToLayer("Combustible");

        // Create the particles
        particles = Instantiate(particles, transform.position, Quaternion.LookRotation(transform.up));
        ParticleSystem.MainModule main;

        if (transform.childCount > 0)
            main = particles.transform.GetChild(0).GetComponent<ParticleSystem>().main;
        else
            main = particles.GetComponent<ParticleSystem>().main;

        main.loop = true;

        particles.transform.position = pos_particles;

        _colls = new Collider[20];
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void StartBurn(float damage_per_second, float apply_range)
    {
        range = apply_range;
        StartCoroutine(DamagePerSecond(damage_per_second));
    }

    private void Update()
    {
        CheckForBalloons();
        ManageFireLife();
    }

    private void CheckForBalloons()
    {
        // Check for balloons
        int count = Physics.OverlapSphereNonAlloc(transform.position, range, _colls);
        if (count > 0)
        {
            foreach (Collider coll in _colls)
            {
                if (coll == null) break;

                if (coll.gameObject.layer == LayerMask.NameToLayer("Burnable"))
                {
                    SC_Interactable interactable = coll.GetComponent<SC_Interactable>();
                    if (interactable != null && !affected_objects.Contains(interactable))
                    {
                        affected_objects.Add(interactable);
                        interactable.Activate();
                    }
                }
                else if (coll.CompareTag("Arrow"))
                {
                    SC_Arrow arrow = coll.GetComponent<SC_Arrow>();
                    if (arrow != null)
                        arrow.SetElement(ElementTypes.FIRE);
                }
            }
        }
    }

    private void ManageFireLife()
    {
        current_life -= Time.deltaTime;

        // Check if there is combustible nearby
        if (current_life < max_life && Physics.Raycast(transform.position, -transform.up, out RaycastHit hit, 3, combustible_mask))
        {
            current_life += Time.deltaTime * 2;
            Debug.DrawLine(transform.position, transform.position + -transform.up * 1.25f, Color.green);
        }
        else
        {
            Debug.DrawLine(transform.position, transform.position + -transform.up * 1.25f, Color.red);
        }
        // Destroy the fire if there isn't any combustible nearby
        if (current_life <= 0)
        {
            Destroy(gameObject);
        }
    }


    protected override void OnDestroy()
    {
        if (particles == null) return;

        ParticleSystem parts = particles.GetComponent<ParticleSystem>();
        parts.Stop();
        Destroy(particles, 5);

        foreach (SC_Interactable inter in affected_objects)
        {
            inter.Deactivate();
        }
    }

    IEnumerator DamagePerSecond(float damage_per_second)
    {
        while (gameObject.activeSelf)
        {
            Collider[] colls = Physics.OverlapSphere(transform.position, range * 0.25f);
            foreach (Collider coll in colls)
            {
                SC_LifeSystem lf = coll.GetComponent<SC_LifeSystem>();
                if (lf != null)
                {
                    lf.Damage(damage_per_second);
                    lf.ApplyElement(SC_ElementManager.instance.GetElement(ElementTypes.FIRE));
                }
            }
            yield return new WaitForSeconds(1);
        }
    }
}