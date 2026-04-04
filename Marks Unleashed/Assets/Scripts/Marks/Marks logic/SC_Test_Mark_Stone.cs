using TMPro;
using UnityEngine;
using System.Collections;

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

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
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
    }

    private void OnCollisionEnter(Collision collision)
    {
        _playerRb = collision.gameObject.GetComponent<Rigidbody>();
    }
}