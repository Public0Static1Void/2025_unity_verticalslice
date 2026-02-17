using TMPro;
using UnityEngine;

public class SC_Test_Mark_Stone : MonoBehaviour
{
    private HingeJoint _hinge;

    private void Awake()
    {
        _hinge = GetComponent<HingeJoint>();
        _hinge.useMotor = false;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            _hinge.useMotor = true;
        }
    }

    private void FixedUpdate()
    {
        if (_hinge.useMotor)
        {
            if (Mathf.Abs(_hinge.angle + 90f) < 1f)
            {
                _hinge.useMotor = false;
            }
        }
    }
}