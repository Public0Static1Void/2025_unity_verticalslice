using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static UnityEngine.ParticleSystem;

public class SC_Test_Mark_Stone : MonoBehaviour
{
    public float rotDuration = 0.5f; //Cuanto tiempo tarda en hacer el giro (seg? min?)
    public float rotAngle = -90f;

    private Rigidbody _playerRb;
    private Rigidbody _rb;
    [HideInInspector] public Quaternion _startRot;
    [HideInInspector] public Quaternion _targetRot;

    public bool can_move = true;
    public float throwForce = 10;

    public bool only_demostration = false;

    [Header("References")]
    public Material mat_decompose;
    public GameObject ob_BrokenRock;
    public AudioClip clip_break;
    public SC_Mark_Stone_Visualizer sc_Mark_Stone_Visualizer;

    [Header("Break Explosion")]
    public float force_explosion = 10000;
    public List<Rigidbody> _rbs_stone_pieces;

    private Material _mat_hologram;
    private Coroutine routine_dissolve = null;

    private bool active = false;


    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();

        if (!only_demostration)
        {
            _startRot = _rb.rotation;
        }
        
        _mat_hologram = GetComponentInChildren<MeshRenderer>().material;
    }
    private void Start()
    {
        if (transform.parent != null)
        {
            int layer = ~LayerMask.GetMask("Player");
            float dist = 9;
            if (Physics.Raycast(transform.parent.position + transform.parent.forward, transform.parent.up, out RaycastHit hit, dist, layer))
            {
                Debug.Log($"Hit {hit.distance}");
                dist = hit.distance;
            }

            transform.position = transform.parent.position + transform.parent.up * (9 - (9 - dist));
            //transform.rotation = transform.parent.rotation;
        }
    }

    private void OnEnable()
    {
        ActivatePillar();
    }

    #region Movement
    public void ActivatePillar()
    {
        if (active) return;
        active = true;
        StopAllCoroutines();

        StartCoroutine(ActivatePillarRoutine());
    }
    public void DeactivatePillar()
    {
        if (!active) return;
        active = false;
        StopAllCoroutines();
        if (only_demostration)
        {
            StartCoroutine(DissolveHologram(1, 1));
        }
    }
    private IEnumerator ActivatePillarRoutine()
    {
        if (only_demostration)
        {
            _startRot = transform.parent.rotation;
            _rb.rotation = _startRot;

            StartCoroutine(DissolveHologram(0.5f, 0.1f));
            yield return new WaitForSeconds(0.4f);
            
            Vector3 rotAxis = transform.parent != null
                ? transform.parent.right
                : transform.right;
            _targetRot = Quaternion.AngleAxis(rotAngle, rotAxis) * _startRot;
        }
        else 
        {
            _targetRot = _startRot * Quaternion.Euler(rotAngle, 0f, 0f);
        }

        float elapsed = 0f;
        while (elapsed < rotDuration) 
        {           
            if (can_move)
            {
                elapsed += Time.fixedDeltaTime;
                float t = elapsed / rotDuration;

                Quaternion newRot = Quaternion.Slerp(_startRot, _targetRot, t);
                _rb.MoveRotation(newRot);
            }

            yield return new WaitForFixedUpdate();
        }
        _rb.MoveRotation(_targetRot);
        

        _rb.linearVelocity = Vector3.zero;

        if (!only_demostration)
            StartCoroutine(EsperarRomper());
        else /// Repeats the movement if demostration is on
        {
            StartCoroutine(DissolveHologram(1, 1));
            yield return new WaitForSeconds(2); /// Waits a bit to avoid unending movement
            active = false;
            ActivatePillar();
        }
    }

    private IEnumerator MovePlayerRoutine(Transform tr, Rigidbody p_rb, int total_points, List<Vector3> points)
    {
        Collider[] colls = _playerRb.transform.GetComponents<Collider>();
        foreach (Collider coll in colls)
        {
            coll.enabled = false;
        }
        p_rb.useGravity = false;


        float timer = 0;
        float duration = 0.1f;

        for (int i = 1; i < total_points; i++)
        {
            Vector3 start_pos = tr.position;
            while (timer < duration)
            {
                float t = timer / duration;
                tr.position = Vector3.Lerp(start_pos, points[i], t);

                timer += Time.deltaTime;
                yield return null;
            }
            timer = 0;
            start_pos = tr.position;
        }

        foreach (Collider coll in colls)
        {
            coll.enabled = true;
        }
        p_rb.useGravity = true;
    }

    private IEnumerator EsperarRomper()
    {
        yield return new WaitForSeconds(2);
        ob_BrokenRock.transform.SetParent(null, true);
        ob_BrokenRock.SetActive(true);
        gameObject.SetActive(false);

        for (int i = 0; i < ob_BrokenRock.transform.childCount; i++)
        {
            SC_CoroutineRunner.instance.StartCoroutine(DecreaseTilLDieRoutine(ob_BrokenRock.transform.GetChild(i)));
        }

        Vector3 random_offset = new Vector3(
                Random.Range(-2f, 2f),
                Random.Range(-2f, 2f),
                Random.Range(-2f, 2f)
            );

        foreach (Rigidbody rb in _rbs_stone_pieces)
        {
            rb.WakeUp();
            Vector3 dir = (rb.transform.position - (transform.position + random_offset)).normalized;
            rb.AddForce(dir * force_explosion, ForceMode.Impulse);
        }
        
        Destroy(ob_BrokenRock, 20);

        SC_SoundManager.instance.InstantiateSound(clip_break, transform.position);
    }
    private IEnumerator DecreaseTilLDieRoutine(Transform tr)
    {
        float timer = 0;
        float duration = Random.Range(1.5f, 3.5f);

        Vector3 start_scale = tr.localScale;

        while (timer < duration)
        {
            float t = timer / duration;
            t = Mathf.SmoothStep(0, 1, t);

            if (tr == null) break;
            tr.localScale = Vector3.Lerp(start_scale, Vector3.zero, t);

            timer += Time.deltaTime;
            yield return null;
        }

        if (tr != null)
            Destroy(tr.gameObject);
    }
    /// <summary>
    /// Makes the dissolve animation. 0.1f full visible - 1 invisible
    /// </summary>
    private IEnumerator DissolveHologram(float duration, float dissolve_amount, bool setActive = true)
    {
        float timer = 0;
        float current_dissolve = _mat_hologram.GetFloat("_DissolveAmount");
        while (timer < duration)
        {
            float t = timer / duration;
            t = Mathf.SmoothStep(0, 1, t);
            float amount = dissolve_amount > 0.1f ? t : 1 - t;
            float lerp = Mathf.Lerp(current_dissolve, amount, t);
            _mat_hologram.SetFloat("_DissolveAmount", lerp);

            timer += Time.deltaTime;
            yield return null;
        }

        _mat_hologram.SetFloat("_DissolveAmount", dissolve_amount);
        gameObject.SetActive(setActive);

        routine_dissolve = null;
    }
    #endregion

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.CompareTag("Player"))
            _playerRb = collision.gameObject.GetComponent<Rigidbody>();
    }
}