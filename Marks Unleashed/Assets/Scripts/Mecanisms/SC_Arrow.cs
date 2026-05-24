using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.ParticleSystem;

[RequireComponent(typeof(SC_OrientToMovement))]
public class SC_Arrow : MonoBehaviour
{
    SC_OrientToMovement _orient;

    private Transform _parent = null;

    public ElementTypes type_element;
    public float hit_force = 100;

    private LayerMask _layer_burnable;
    private float _range_burn = 1;

    private List<string> _affected_names = new List<string>();

    private void Start()
    {
        _orient = GetComponent<SC_OrientToMovement>();

        _layer_burnable = 1 << LayerMask.NameToLayer("Combustible");

        Destroy(transform.parent != null ? transform.parent.gameObject : gameObject, 10);
    }

    private void Update()
    {
        if (type_element == ElementTypes.FIRE)
        {
            Collider[] colls = Physics.OverlapSphere(transform.position, _range_burn, _layer_burnable);
            if (colls.Length > 0)
            {
                foreach (Collider coll in colls)
                {
                    if (!_affected_names.Contains(coll.name))
                    {
                        SC_ZoneBurnable zb = coll.GetComponent<SC_ZoneBurnable>();
                        if (zb != null)
                            zb.Ignite(transform.position, 6, 1);

                        _affected_names.Add(coll.name);
                    }
                }
            }
        }
    }

    public void SetElement(ElementTypes type)
    {
        type_element = type;
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Freeze movement and attach to the collisioned object
        if (_parent == null)
        {
            _orient._rb.isKinematic = true;
            transform.SetParent(collision.transform, true);

            GetComponent<Collider>().enabled = false;
        }
        
    }
    private void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.gameObject.GetComponent<SC_PlayerLife>().Damage(4);
            collision.gameObject.GetComponent<Rigidbody>().AddForce(transform.forward * hit_force, ForceMode.Impulse);
            SC_CameraRotation.instance.ShakeCamera(1, 0.25f);
        }
    }
}
