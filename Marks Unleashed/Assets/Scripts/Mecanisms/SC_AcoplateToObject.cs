using UnityEngine;

public class SC_AcoplateToObject : MonoBehaviour
{
    public Transform target;

    void Update()
    {
        transform.position = target.transform.position + target.transform.up * 3;
    }
}