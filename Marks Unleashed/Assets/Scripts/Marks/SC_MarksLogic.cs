using UnityEngine;
using System.Collections;

public class Mark_Water : SC_Mark
{
    public float range = 3;

    public override void Activate()
    {
        Destroy(Instantiate(particles, transform.position, transform.rotation), 5);

        Collider[] colls = Physics.OverlapSphere(transform.position, range, layer_interactable);
        foreach (Collider coll in colls)
        {
            Debug.Log(coll.name);
            SC_LifeSystem lifeSystem = coll.GetComponent<SC_LifeSystem>();
            if (lifeSystem != null)
            {
                lifeSystem.ApplyElement(SC_ElementManager.instance.elements[(int)ElementTypes.WATER]);
            }

            SC_ObjectMovement objectMovement = coll.GetComponent<SC_ObjectMovement>();
            if (objectMovement != null)
            {
                objectMovement.Slow(1 - element.strength * 0.1f, element.strength * 1.5f);
            }
        }

        Destroy(gameObject);
    }
}
public class Mark_Electricity : SC_Mark
{
    public float range = 3;

    public override void Activate()
    {
        Destroy(Instantiate(particles, transform.position, transform.rotation), 5);

        Collider[] colls = Physics.OverlapSphere(transform.position + transform.up * 50, range, layer_interactable);
        foreach (Collider coll in colls)
        {
            SC_LifeSystem lifeSystem = coll.GetComponent<SC_LifeSystem>();
            if (lifeSystem != null)
            {
                lifeSystem.ApplyElement(SC_ElementManager.instance.elements[(int)ElementTypes.ELECTRICITY]);
            }

            SC_ObjectMovement movementScript = coll.GetComponent<SC_ObjectMovement>();
            if (movementScript != null)
            {
                movementScript.StopMovement(2);
            }
        }

        Destroy(gameObject);
    }
}
public class Mark_Fire : SC_Mark
{
    public float range = 3;

    public override void Activate()
    {
        Destroy(Instantiate(particles, transform.position, transform.rotation), 5);

        Destroy(gameObject);
    }
}
public class Mark_Stone : SC_Mark
{
    public float range = 3;

    public override void Activate()
    {
        Destroy(Instantiate(particles, transform.position, transform.rotation), 5);

        Destroy(gameObject);
    }
}
public class Mark_Air : SC_Mark
{
    public float range = 3;

    public override void Activate()
    {
        Destroy(Instantiate(particles, transform.position, transform.rotation), 5);

        Collider[] colls = Physics.OverlapSphere(transform.position, range, layer_interactable);
        foreach (Collider coll in colls)
        {
            SC_ObjectMovement objectMovement = coll.GetComponent<SC_ObjectMovement>();
            if (objectMovement != null)
            {
                objectMovement.rb.AddForce(transform.up * element.strength * 10, ForceMode.Impulse);
            }
            SC_LifeSystem life = coll.GetComponent<SC_LifeSystem>();
            if (life != null)
            {
                life.ApplyElement(element);
            }
        }

        Destroy(gameObject);
    }
}
public class Mark_Gravity : SC_Mark
{
    public float range = 3;

    public override void Activate()
    {
        Destroy(Instantiate(particles, transform.position, transform.rotation), 5);

        Destroy(gameObject);
    }
}