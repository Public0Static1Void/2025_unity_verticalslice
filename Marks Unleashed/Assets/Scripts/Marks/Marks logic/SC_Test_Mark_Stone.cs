using TMPro;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SC_Test_Mark_Stone : MonoBehaviour
{
    public float rotDuration = 0.5f; //Cuanto tiempo tarda en hacer el giro (seg? min?)
    public float rotAngle = -90f;

    private Rigidbody _playerRb;
    private Rigidbody _rb;
    private Quaternion _startRot;
    private Quaternion _targetRot;

    public bool can_move = true;
    public float throwForce = 10;

    [Header("References")]
    public Material mat_decompose;
    public GameObject ob_BrokenRock;
    public AudioClip clip_break;

    private MeshRenderer _meshRenderer;

    [Header("Break Explosion")]
    public float force_explosion = 10000;
    public List<Rigidbody> _rbs_stone_pieces;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _meshRenderer = GetComponentInChildren<MeshRenderer>();
    }

    private void OnEnable()
    {
        StartCoroutine(ActivatePillar());
    }

    private IEnumerator ActivatePillar()
    {
        _startRot = _rb.rotation;
        _targetRot = _startRot * Quaternion.Euler(rotAngle, 0f, 0f);

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
        if (_playerRb != null)
        {
            _playerRb.AddForce(-transform.up * throwForce, ForceMode.Impulse);
        }

        _rb.linearVelocity = Vector3.zero;

        
        StartCoroutine(EsperarRomper());
    }

    private IEnumerator DecreaseTilLDieRoutine(Transform tr)
    {
        float timer = 0;
        float duration = Random.Range(5, 8);

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

    private void OnCollisionEnter(Collision collision)
    {
        _playerRb = collision.gameObject.GetComponent<Rigidbody>();
    }
}