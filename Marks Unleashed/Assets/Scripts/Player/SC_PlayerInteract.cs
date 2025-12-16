using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

public class SC_PlayerInteract : MonoBehaviour
{
    public TMP_Text ui_text;

    public float interact_range = 3;
    public LayerMask layer_interactable;

    public float text_show_speed = 3;

    private string text = "Press E to interact";

    private Collider nearest_coll = null;

    private void Update()
    {
        Collider[] colls = Physics.OverlapSphere(transform.position, interact_range, layer_interactable);
        if (colls.Length > 0)
        {
            ui_text.text = text;

            Color col = ui_text.color;
            ui_text.color = Color.Lerp(ui_text.color, new Color(col.r, col.g, col.b, 1), Time.deltaTime * text_show_speed);
            foreach (Collider coll in colls)
            {
                if (nearest_coll == null)
                    nearest_coll = coll;
                else if (Vector3.Distance(transform.position, nearest_coll.transform.position) > Vector3.Distance(transform.position, coll.transform.position))
                {
                    nearest_coll = coll;
                }
            }
        }
        else
        {
            Color col = ui_text.color;
            ui_text.color = Color.Lerp(ui_text.color, new Color(col.r, col.g, col.b, 0), Time.deltaTime * text_show_speed);
        }
    }
    private void Interact()
    {
        if (nearest_coll == null) return;

        IN_Interactable interactable = nearest_coll.gameObject.GetComponent<IN_Interactable>();
        if (interactable != null)
        {
            interactable.Activate();
        }
    }
    public void InteractInput(InputAction.CallbackContext con)
    {
        if (con.performed)
        {
            Interact();
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, interact_range);
    }
}
