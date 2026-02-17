using TMPro;
using UnityEngine;

public class SC_Test_Mark_Stone : MonoBehaviour
{
    public float finalHeight = 4f;      //How high will the pillar rise
    public float speed = 8f;
    public float impulseForce = 10f;

    private Vector3 _startPosition;
    private Vector3 _targetPosition;

    private bool _isActivated;
    
    private Rigidbody _rb;

    private void Start()
    {
        _rb = GetComponent<Rigidbody>();

        _startPosition = transform.position;
        _targetPosition = _startPosition + Vector3.up * finalHeight;
    }

    private void Update()
    {
        // Temporary test activation
        if (Input.GetKeyDown(KeyCode.R))
        {
            Activate();
        }

        // Temporary test activation
        if (Input.GetKeyDown(KeyCode.T))
        {
            ResetPosition();
        }
    }

    private void FixedUpdate()
    {
        if (!_isActivated) return;

        Vector3 nextPosition = Vector3.MoveTowards(
            _rb.position,
            _targetPosition,
            speed * Time.fixedDeltaTime
        );

        _rb.MovePosition(nextPosition);

        if (Vector3.Distance(_rb.position, _targetPosition) < 0.01f)
        {
            _isActivated = false;
        }
    }
    

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Rigidbody playerRb = collision.gameObject.GetComponent<Rigidbody>();

            if (playerRb != null) 
            {
                playerRb.AddForce(Vector3.up * impulseForce, ForceMode.Impulse);
            }
        }
    }
    public void Activate()
    {
        _isActivated = true;
    }

    public void ResetPosition()
    {
        transform.position = _startPosition - Vector3.up * finalHeight;
        _isActivated = false;
    }
}