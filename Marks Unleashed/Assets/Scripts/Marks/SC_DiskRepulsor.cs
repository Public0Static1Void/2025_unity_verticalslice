using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SC_DiskRepulsor : MonoBehaviour
{
    public float force, radius;
    public LayerMask _layer_disk;

    private List<Rigidbody> _affected_rb;

    private Material _mat;

    private Coroutine _changeColorRoutine;

    Collider[] _colls;

    private void Start()
    {
        _affected_rb = new List<Rigidbody>();
        _mat = GetComponent<MeshRenderer>().material;

        _colls = new Collider[10]; // Prepare to recieve a max of 10 colliders at a time
    }

    private void FixedUpdate()
    {
        _affected_rb.Clear();
        int count = Physics.OverlapSphereNonAlloc(transform.position, radius, _colls, _layer_disk);
        if (count > 0)
        {
            foreach (Collider coll in _colls)
            {
                if (coll == null) break;
                // Gets the attached rb without calling getComponent
                Rigidbody rb = coll.attachedRigidbody;
                if (rb != null)
                    _affected_rb.Add(coll.attachedRigidbody);
            }
            ApplyForces();

            if (_changeColorRoutine == null)
                _changeColorRoutine = StartCoroutine(ChangeColorRoutine());
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