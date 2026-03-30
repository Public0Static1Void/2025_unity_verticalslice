using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class SC_MarksSelector : MonoBehaviour
{
    public LayerMask layer_marks;

    public SC_Zone selected_mark_zone;

    public GameObject mark_selector_indicator;

    public AudioClip clip_remove_runes;

    void Start()
    {
        mark_selector_indicator = Instantiate(mark_selector_indicator, transform.position, Quaternion.identity);
        mark_selector_indicator.SetActive(false);
        mark_selector_indicator.transform.localScale = Vector3.one * 0.03f;
    }

    void FixedUpdate()
    {
        if (Physics.SphereCast(transform.position, 0.5f, transform.forward, out RaycastHit hit, 50, layer_marks))
        {
            selected_mark_zone = hit.collider.gameObject.GetComponent<SC_Zone>();
            if (selected_mark_zone != null)
            {
                if (selected_mark_zone.destroyable)
                {
                    selected_mark_zone.HighlightZone();
                    if (mark_selector_indicator != null)
                    {
                        mark_selector_indicator.SetActive(true);
                        mark_selector_indicator.transform.position = selected_mark_zone.transform.position + Vector3.up * 3;
                        mark_selector_indicator.transform.LookAt(transform);
                    }
                }
                else
                {
                    selected_mark_zone = null;
                    mark_selector_indicator.SetActive(false);
                }
            }
        }
        else
        {
            selected_mark_zone = null;
            mark_selector_indicator.SetActive(false);
        }
    }

    public void ProcessDeleteZoneInput(InputAction.CallbackContext con)
    {
        if (con.performed && selected_mark_zone != null && SC_MarksPlacer.Instance.placed_marks.Count <= 0)
        {
            SC_MarksPlacer.Instance.RemoveMarks();
            SC_SoundManager.instance.InstantiateSound(clip_remove_runes, transform.position);
        }
    }
}