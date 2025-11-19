using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System.Collections;

public class SC_MarksPlacer : MonoBehaviour
{
    [Header("References")]
    public GameObject mark_placeholder;
    public Transform player;
    public LayerMask layer_placeable;

    // Marks selection
    public GameObject marks_canvas;
    private List<Image> marks_list = new List<Image>();

    [Header("Stats")]
    public float range_placing = 2;

    [Header("Marks")]
    public Element equipped_mark;

    public List<GameObject> placed_marks;

    private bool placing_mark = false;

    private void Start()
    {
        StartCoroutine(InitMarksUI());
    }

    private IEnumerator InitMarksUI()
    {
        yield return null;

        // Add to the list the marks slots and set it's sprites
        int marks_count = SC_ElementManager.instance.elements.Length;
        for (int i = 0; i < marks_count; i++)
        {
            GameObject ob = new GameObject($"Marks place {i + 1}");
            ob.transform.SetParent(marks_canvas.transform, false);

            Image im = ob.AddComponent<Image>();
            im.sprite = SC_ElementManager.instance.elements[i].im_mark_sprite;

            // Place it around the parent
            float angle = i * Mathf.PI * 2 / marks_count; /// Get the angle where the mark will be
            float x = Mathf.Cos(angle) * 1;
            float y = Mathf.Sin(angle) * 1;

            RectTransform rect = ob.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(x, y);

            marks_list.Add(im);
        }
    }

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
            inst.GetComponent<SC_Mark>().image.sprite = equipped_mark.im_mark_sprite;

            placed_marks.Add(inst);
        }
    }

    private void OpenMarkSelectionMenu()
    {
        marks_canvas.SetActive(true);
        // Aquí hay que poner que haga animación etc
    }
    private void ScrollMarksList(int direction)
    {
        if (marks_list.Count <= 1) return;

        if (direction > 0) // Scroll up
        {
            int last_inx = marks_list.Count - 1;
            Image last = marks_list[last_inx];
            marks_list.RemoveAt(last_inx);
            marks_list.Insert(0, last);
        }
        else if (direction < 0) // Scroll down
        {
            Image first = marks_list[0];
            marks_list.RemoveAt(0);
            marks_list.Add(first);
        }

        RefreshMarksList();
    }
    private void RefreshMarksList()
    {
        if (marks_list.Count == 0) return;
        for (int i = 0; i < marks_list.Count; i++)
        {
            marks_list[i].transform.SetSiblingIndex(i);
            marks_list[i].rectTransform.sizeDelta = Vector2.one * 0.5f;
        }


        marks_list[0].rectTransform.sizeDelta = Vector2.one * 0.6f;
    }

    #region Inputs
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
    public void OpenMarksMenu(InputAction.CallbackContext con)
    {
        if (con.performed)
        {
            OpenMarkSelectionMenu();
        }
    }
    public void NavigateOnMarkMenu(InputAction.CallbackContext con)
    {
        if (con.performed && marks_canvas.activeSelf)
        {
            float y_scroll = con.ReadValue<Vector2>().y;
            if (y_scroll < 0) y_scroll = -1;
            else if (y_scroll > 0) y_scroll = 1;

            ScrollMarksList((int)y_scroll);
        }
    }
    public void ActivateMarks(InputAction.CallbackContext con)
    {
        if (con.performed)
        {
            ActivateMarks();
        }
    }
    #endregion

    // Gizmos
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(player.position, range_placing);
    }
}