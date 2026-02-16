using TMPro;
using UnityEngine;

public class SC_Test_Mark_Stone : MonoBehaviour
{
    public float finalHeight = 4f;  //How high will the pillar rise
    public float speed = 8f;        //How fast will it move

    private Vector3 _startPosition;
    private Vector3 _targetPosition;

    private bool _isActivated;

    private void Start()
    {
        _startPosition = transform.position;
        _targetPosition = _startPosition + Vector3.up * finalHeight;

        //Automatically places pillar below the floor at start
        transform.position = _startPosition - Vector3.up * finalHeight;
    }

    private void Update()
    {
        // Temporary test activation
        if (Input.GetKeyDown(KeyCode.R))
        {
            Activate();
        }

        if (!_isActivated) return;

        transform.position = Vector3.MoveTowards(
           transform.position,
           _targetPosition,
           speed * Time.deltaTime
       );
    }

    public void Activate()
    {
        _isActivated = true;
    }
}