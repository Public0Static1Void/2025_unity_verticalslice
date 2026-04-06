using UnityEngine;

public class SC_AdjustLocation : MonoBehaviour
{
    public Transform target;
    public float follow_speed;
    public Vector3 offset;
    public float dist = 2;


    // Update is called once per frame
    [SerializeField] Camera cam;
    [SerializeField] float minY = 0.2f; // bottom limit in screen
    [SerializeField] float maxY = 0.8f; // top limit

    void LateUpdate()
    {
        if (target == null) return;

        // Base position (your logic)
        Vector3 pos = target.position + target.forward * offset.z;

        // Convert to viewport space
        Vector3 viewPos = cam.WorldToViewportPoint(pos);

        // Convert back to world space
        Vector3 worldPos = cam.ViewportToWorldPoint(viewPos);

        // Apply only Y
        transform.position = new Vector3(
            transform.position.x,
            worldPos.y,
            transform.position.z
        );

        // Rotation (your existing logic)
        Vector3 dir = target.position - transform.position;
        transform.rotation = Quaternion.LookRotation(-dir);
    }
}
