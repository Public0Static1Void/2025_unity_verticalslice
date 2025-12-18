using UnityEngine;

public class SC_RotateTowardsTarget : MonoBehaviour
{
    public Transform target;
    
    void Update()
    {
        transform.LookAt(target, Vector3.up);
    }
}