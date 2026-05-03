using UnityEngine;

public class SC_Rotate : MonoBehaviour
{
    public Vector3 rotation;

    void Update()
    {
        transform.Rotate(rotation * Time.deltaTime, Space.Self);
    }
}