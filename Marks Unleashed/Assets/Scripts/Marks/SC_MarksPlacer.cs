using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class SC_MarksPlacer : MonoBehaviour
{
    [Header("References")]
    public GameObject mark_placeholder;
    public Transform player;
    public LayerMask layer_placeable;

    [Header("Stats")]
    public float range_placing = 2;

    [Header("Marks")]
    public Element equipped_mark;

    public List<GameObject> placed_marks;

    private bool placing_mark = false;


    private void Update()
    {
        if (placing_mark)
        {
            
        }
    }
    private void ActivateMarks()
    {
        for (int i = 0; i < placed_marks.Count; i++)
        {
            placed_marks[i].GetComponent<SC_Mark>().Activate();
        }
    }

    private void PlaceMarkOnSurface()
    {
        if (equipped_mark == null) return;

        if (Physics.Raycast(player.position, player.forward, out RaycastHit hit, range_placing, layer_placeable))
        {
            GameObject inst = Instantiate(mark_placeholder, hit.point, Quaternion.LookRotation(player.position, Vector3.up));
            inst.GetComponent<Image>().sprite = equipped_mark.im_mark_sprite;

            placed_marks.Add(inst);
        }
    }
    public void PlaceMark(InputAction.CallbackContext con)
    {
        if (con.performed)
        {
            if (equipped_mark != null)
                placing_mark = true;
        }
        else if (con.canceled)
        {
            placing_mark = false;
            PlaceMarkOnSurface();
        }
    }

    public void ActivateMarks(InputAction.CallbackContext con)
    {
        if (con.performed)
        {
            ActivateMarks();
        }
    }

    // Gizmos
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(player.position, range_placing);
    }
}