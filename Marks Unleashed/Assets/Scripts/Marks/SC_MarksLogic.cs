using UnityEngine;
using System.Collections;

public class Mark_Water : SC_Mark
{
    public float range = 3;

    public override void Activate()
    {
        GameObject ob = Instantiate(particles, transform.position, transform.rotation);
        ob.transform.GetChild(3).SetParent(null, true);
        Destroy(ob, 5);

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
                objectMovement.Slow(1 - element.strength * 0.25f, element.strength * 1.5f);
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
        Destroy(Instantiate(particles, transform.position + transform.up * 2, transform.rotation), 5);

        Collider[] colls = Physics.OverlapSphere(transform.position, range, layer_interactable);
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
                Vector3 dir = (coll.transform.position - transform.position).normalized;
                objectMovement.rb.AddForce(dir * element.strength * 250, ForceMode.Impulse);
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
    public float range = 10;

    public override void Activate()
    {
        Destroy(Instantiate(particles, transform.position, transform.rotation), 5);

        Collider[] colls = Physics.OverlapSphere(transform.position, range, layer_interactable);
        foreach (Collider coll in colls)
        {
            SC_ObjectMovement objectMovement = coll.GetComponent<SC_ObjectMovement>();
            if (objectMovement != null)
            {
                Vector3 dir = (transform.position - coll.transform.position).normalized;
                objectMovement.rb.AddForce(dir * element.strength * 400, ForceMode.Impulse);
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