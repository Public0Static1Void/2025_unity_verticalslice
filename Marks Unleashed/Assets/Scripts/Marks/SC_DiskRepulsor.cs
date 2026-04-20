using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SC_DiskRepulsor : MonoBehaviour
{
    public float force, radius;
    public LayerMask _layer_disk;

    private List<Rigidbody> _affected_rb;
    private List<string> _names_in;
    private List<SC_MarkDisk> _disks_in;

    private Material _mat;

    private Coroutine _changeColorRoutine;

    Collider[] _colls;

    private void Start()
    {
        // Init the lists
        _affected_rb = new List<Rigidbody>();
        _names_in = new List<string>();
        _disks_in = new List<SC_MarkDisk>();

        _mat = GetComponent<MeshRenderer>().material;

        _colls = new Collider[10]; // Prepare to recieve a max of 10 colliders at a time
    }

    private void FixedUpdate()
    {
        int count = Physics.OverlapSphereNonAlloc(transform.position, radius, _colls, _layer_disk);
        if (count > 0)
        {
            List<string> objects_in = new List<string>();

            for (int i = 0; i < count; i++)
            {
                Collider coll = _colls[i];
                if (coll == null) break;

                objects_in.Add(coll.name);

                /// Ignore if the object was already there
                if (_names_in.Contains(coll.name)) continue;
                
                /// Gets the attached rb without calling GetComponent
                Rigidbody rb = coll.attachedRigidbody;
                if (rb != null)
                {
                    _affected_rb.Add(coll.attachedRigidbody);
                    _names_in.Add(coll.name);

                    /// Make the rune impossible to place inside
                    SC_MarkDisk disk = coll.GetComponent<SC_MarkDisk>();
                    disk.can_spawn = false;
                    _disks_in.Add(disk);
                }
            }

            // Check wich objects are still there
            string[] names_copy = _names_in.ToArray();
            SC_MarkDisk[] disks_copy = _disks_in.ToArray();
            for (int i = 0; i < names_copy.Length; i++)
            {
                if (!objects_in.Contains(names_copy[i]))
                {
                    _names_in.RemoveAt(i);
                    _affected_rb.RemoveAt(i);
                    disks_copy[i].can_spawn = true;
                    _disks_in.RemoveAt(i);
                }
            }

            ApplyForces();

            if (_changeColorRoutine == null)
                _changeColorRoutine = StartCoroutine(ChangeColorRoutine());
        }
        else
        {
            _affected_rb.Clear();
            if (_disks_in.Count > 0)
            {
                for (int i = 0; i <  _disks_in.Count; i++)
                    _disks_in[i].can_spawn = true;
                _disks_in.Clear();
            }
            _names_in.Clear();
        }
    }
    private void ApplyForces()
    {
        if (_affected_rb.Count <= 0) return;

        foreach (Rigidbody rb in _affected_rb)
        {
            Vector3 diff = rb.transform.position - transform.position;
            Vector3 dir = diff.normalized;
            float dist = diff.magnitude;

            float normalizedDist = dist / radius;
            float strength = force * (1f / (normalizedDist * normalizedDist + 0.01f));

            rb.AddForce(dir * strength, ForceMode.Acceleration);
        }
    }


    private IEnumerator ChangeColorRoutine()
    {
        float duration = 1;
        float timer = 0;

        Color start_col = Color.white;
        Color end_col = Color.red;

        while (timer < duration * 0.25f)
        {
            float t = timer / duration;
            t = Mathf.SmoothStep(0, 1, t);

            _mat.SetColor("_MainColor", Color.Lerp(start_col, end_col, t));

            timer += Time.deltaTime;
            yield return null;
        }
        timer = 0;
        while (timer < duration * 0.75f)
        {
            float t = timer / duration;
            t = Mathf.SmoothStep(0, 1, t);

            _mat.SetColor("_MainColor", Color.Lerp(end_col, start_col, t));

            timer += Time.deltaTime;
            yield return null;
        }

        _mat.SetColor("_MainColor", start_col);

        _changeColorRoutine = null;
    }


    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}