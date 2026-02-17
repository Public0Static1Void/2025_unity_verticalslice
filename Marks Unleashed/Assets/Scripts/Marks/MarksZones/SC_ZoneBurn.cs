using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SC_ZoneBurn : MonoBehaviour
{
    // Stats
    [Header("Stats")]
    public float range = 6;
    public float max_life = 3;
    private float current_life = 3;

    // References
    [Header("References")]
    public GameObject parts_fire;


    private List<SC_Interactable> affected_objects = new List<SC_Interactable>();

    private LayerMask balloon_mask, combustible_mask;



    private void Start()
    {
        current_life = max_life;

        balloon_mask = 1 << LayerMask.NameToLayer("Burnable");
        combustible_mask = 1 << LayerMask.NameToLayer("Combustible");

        // Create the particles
        parts_fire = Instantiate(parts_fire, transform.position, Quaternion.LookRotation(transform.up));
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void StartBurn(int damage_per_second, float apply_range)
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
        Collider[] colls = Physics.OverlapSphere(transform.position, range, balloon_mask);
        if (colls.Length > 0)
        {
            foreach (Collider coll in colls)
            {
                SC_Interactable interactable = coll.GetComponent<SC_Interactable>();
                if (interactable != null && !affected_objects.Contains(interactable))
                {
                    affected_objects.Add(interactable);
                    interactable.Activate();
                }
            }
        }
    }

    private void ManageFireLife()
    {
        current_life -= Time.deltaTime;

        // Check if there is combustible nearby
        if (current_life < max_life && Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 0.5f, combustible_mask))
        {
            current_life += Time.deltaTime * 2;
        }
        // Destroy the fire if there isn't any combustible nearby
        if (current_life <= 0)
        {
            Destroy(gameObject);
        }
    }


    private void OnDestroy()
    {
        foreach (SC_Interactable inter in affected_objects)
        {
            inter.Deactivate();
        }

        ParticleSystem parts = parts_fire.GetComponent<ParticleSystem>();
        parts.Stop();
        Destroy(parts_fire, 5);
    }

    IEnumerator DamagePerSecond(int damage_per_second)
    {
        while (gameObject.activeSelf)
        {
            Collider[] colls = Physics.OverlapSphere(transform.position, range);
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