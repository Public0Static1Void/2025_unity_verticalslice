using UnityEngine;
using UnityEngine.InputSystem;

public class SC_MarksSelector : MonoBehaviour
{
    public LayerMask layer_marks;

    public SC_Zone selected_mark_zone;

    void Start()
    {
        
    }

    void FixedUpdate()
    {
        if (Physics.SphereCast(transform.position, 0.5f, transform.forward, out RaycastHit hit, 50, layer_marks))
        {
            selected_mark_zone = hit.collider.gameObject.GetComponent<SC_Zone>();
            if (selected_mark_zone != null)
            {
                selected_mark_zone.HighlightZone();
            }
        }
    }

    public void ProcessDeleteZoneInput(InputAction.CallbackContext con)
    {
        if (con.performed && selected_mark_zone != null)
        {
            Destroy(selected_mark_zone.gameObject);
        }
    }
}