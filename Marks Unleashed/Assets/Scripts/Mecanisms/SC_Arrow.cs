using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.ParticleSystem;

[RequireComponent(typeof(SC_OrientToMovement))]
public class SC_Arrow : MonoBehaviour
{
    private SC_OrientToMovement _orient;

    private Transform _parent = null;

    public ElementTypes type_element;
    public float hit_force = 100;

    private LayerMask _layer_burnable;
    private float _range_burn = 1;

    private List<string> _affected_names = new List<string>();

    [Header("Sounds")]
    public List<AudioClip> clips_hit_normal;
    public List<AudioClip> clips_hit_flesh;

    [Header("Particles")]
    public ParticleSystem parts_hit;

    private void Start()
    {
        _orient = GetComponent<SC_OrientToMovement>();

        _layer_burnable = 1 << LayerMask.NameToLayer("Combustible");

        Destroy(transform.parent != null ? transform.parent.gameObject : gameObject, 30);
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

            Collider[] colls = GetComponents<Collider>();
            foreach (Collider coll in colls)
            {
                if (!coll.isTrigger)
                {
                    coll.enabled = false;
                    break;
                }
            }

            parts_hit.Play();

            Destroy(gameObject, 3);
        }
    }
    private void OnTriggerEnter(Collider collision)
    {
        if (transform.parent != null) return;

        if (collision.gameObject.CompareTag("Player"))
        {
            collision.gameObject.GetComponent<SC_PlayerLife>().Damage(damage: 4, impulse: transform.forward * hit_force);

            SC_SoundManager.instance.InstantiateSound(clips_hit_flesh[Random.Range(0, clips_hit_flesh.Count)], transform.position, pitch: Random.Range(0.8f, 1.2f));
        }
        else
        {
            SC_SoundManager.instance.InstantiateSound(clips_hit_normal[Random.Range(0, clips_hit_normal.Count)], transform.position, volume: 0.25f, pitch: Random.Range(1f, 1.25f));
        }
    }
}