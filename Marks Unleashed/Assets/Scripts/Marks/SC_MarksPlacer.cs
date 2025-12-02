using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Assertions.Must;

public class SC_MarksPlacer : MonoBehaviour
{
    [Header("References")]
    public GameObject mark_placeholder;
    public Image mark_placeholder_image;
    public Transform player;
    public LayerMask layer_placeable;
    
    // Marks activation order UI
    public Transform ui_order_parent;
    private Image[] im_activation_order;

    private List<GameObject> element_hands = new List<GameObject>();

    // Marks selection
    public GameObject marks_canvas;
    private List<GameObject> marks_list = new List<GameObject>();
    private List<ElementTypes> unlocked_marks = new List<ElementTypes>();
    private List<SC_Mark> marks = new List<SC_Mark>();
    private List<Image> marks_list_images = new List<Image>();

    private bool open_menu = false;

    [Header("Stats")]
    public float range_placing = 2;

    [Header("Marks")]
    public Element equipped_mark;

    public List<SC_Mark> placed_marks;

    private int current_mark = 0;

    private bool placing_mark = false;

    private void Start()
    {
        StartCoroutine(InitMarksUI());

        mark_placeholder = Instantiate(mark_placeholder, player.transform.position, Quaternion.identity);
        marks_canvas.SetActive(false);
        mark_placeholder.SetActive(false);
        mark_placeholder.GetComponentInChildren<Canvas>().sortingOrder = -2;

        mark_placeholder_image = mark_placeholder.GetComponentInChildren<Image>();

        // Marks activation order UI
        im_activation_order = SC_GameManager.GetChildsComponents<Image>(ui_order_parent);
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

            ob.SetActive(false);

            SC_Mark mark = ob.AddComponent<SC_Mark>();
            mark = SC_ElementManager.instance.marks[i];
            Debug.Log($"Im: {mark.element.im_mark_sprite}");

            Image im = ob.AddComponent<Image>();
            im.sprite = mark.element.im_mark_sprite;

            // Place it around the parent
            float angle = i * Mathf.PI * 2 / marks_count; /// Get the angle where the mark will be
            float x = Mathf.Cos(angle) * 1;
            float y = Mathf.Sin(angle) * 1;

            RectTransform rect = ob.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(x, y);

            marks_list.Add(ob);
            marks.Add(mark);
            marks_list_images.Add(im);
        }

        equipped_mark = marks[current_mark].element;

        UnlockMark(marks[(int)ElementTypes.WATER].element.el_type);
        UnlockMark(marks[(int)ElementTypes.ELECTRICITY].element.el_type);
        UnlockMark(marks[(int)ElementTypes.GRAVITY].element.el_type);
        UnlockMark(marks[(int)ElementTypes.AIR].element.el_type);
        UnlockMark(marks[(int)ElementTypes.FIRE].element.el_type);

        RefreshMarksList();

        // Load the element hands
        for (int i = 0; i < marks_list.Count; i++)
        {
            element_hands.Add(transform.GetChild(2).GetChild(i).gameObject);
        }
    }

    private void Update()
    {
        if (placing_mark)
        {
            if (Physics.Raycast(player.position, Camera.main.transform.forward, out RaycastHit hit, range_placing))
            {
                mark_placeholder.transform.position = hit.point;
                mark_placeholder.transform.rotation = Quaternion.LookRotation(hit.normal);
            }
            else
            {
                mark_placeholder.transform.position = player.position + Camera.main.transform.forward * range_placing;
                mark_placeholder.transform.LookAt(Camera.main.transform);
            }
        }
    }

    public void UnlockMark(ElementTypes type)
    {
        unlocked_marks.Add(marks[(int)type].element.el_type);
        marks_list_images[(int)type].gameObject.SetActive(true);
    }

    private void UpdateActivateMarksUI()
    {
        for (int i = 0; i < im_activation_order.Length; i++)
        {
            Sprite spr = null;
            if (i < placed_marks.Count && placed_marks[i] != null)
            {
                spr = placed_marks[i].element.im_mark_sprite;
                im_activation_order[i].gameObject.SetActive (true);
            }
            else
            {
                im_activation_order[i].gameObject.SetActive(false);
            }

            im_activation_order[i].sprite = spr;
        }
    }

    /// <summary>
    /// Activates the effects of all the placed marks
    /// </summary>
    private void ActivateMark()
    {
        if (placed_marks.Count <= 0) return;

        placed_marks[0].GetComponent<SC_Mark>().Activate();
        placed_marks.RemoveAt(0);

        UpdateActivateMarksUI();
    }

    /// <summary>
    /// Instantiates a mark on the looking surface and adds it to the placed marks list
    /// </summary>
    private void PlaceMarkOnSurface()
    {
        if (!open_menu || !placing_mark || unlocked_marks.Count <= 0) return;

        if (Physics.Raycast(player.position, Camera.main.transform.forward, out RaycastHit hit, range_placing, layer_placeable))
        {
            OpenMarkSelectionMenu();

            // Get the selected mark info
            SC_Mark mark_info = SC_ElementManager.instance.marks[(int)unlocked_marks[current_mark]];

            // Create canvas parent
            GameObject mark_canvas = new GameObject($"Mark {mark_info.element.name} canvas");
            mark_canvas.transform.position = hit.point;
            mark_canvas.transform.rotation = Quaternion.LookRotation(hit.normal);

            mark_canvas.transform.position -= mark_canvas.transform.forward * 200; // Adds offset from the ground
            Canvas canvas = mark_canvas.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;

            // Resize the canvas
            RectTransform rt = canvas.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(0.5f, 0.5f);
            rt.localScale = Vector3.one;

            // Create the image
            GameObject inst = new GameObject($"Mark {mark_info.element.name}");

            inst.transform.position = hit.point;
            inst.transform.rotation = Quaternion.LookRotation(hit.normal);

            Image im = inst.AddComponent<Image>();
            im.transform.SetParent(canvas.transform);

            im.sprite = mark_info.element.im_mark_sprite;
            im.rectTransform.anchoredPosition = Vector2.zero;
            im.rectTransform.sizeDelta = Vector2.one * 0.5f;
                /// Adds the mark element component
            inst.AddComponent(mark_info.GetType());

            SC_Mark mark = inst.GetComponent(mark_info.GetType()) as SC_Mark;
            mark.particles = mark_info.particles;
            mark.element = mark_info.element;


            placed_marks.Add(mark);

            UpdateActivateMarksUI();

            placing_mark = false;
        }
    }

    /// <summary>
    /// Opens or closes the mark in-game selection menu
    /// </summary>
    private void OpenMarkSelectionMenu()
    {
        open_menu = !open_menu;
        marks_canvas.SetActive(open_menu);

        int index = (int)unlocked_marks[current_mark];
        equipped_mark = marks[index].element;
        mark_placeholder_image.sprite = equipped_mark.im_mark_sprite;
        EnableElementHand(index);

        mark_placeholder.SetActive(open_menu);

        // Aquí hay que poner que haga animación etc
        if (open_menu)
        {
            placing_mark = true;
        }
        else
        {
            EnableElementHand(-1);
            placing_mark = false;
        }
    }

    /// <summary>
    /// Puts on screen the selected element hand
    /// </summary>
    private void EnableElementHand(int index)
    {
        for (int i = 0; i < element_hands.Count; i++)
        {
            element_hands[i].SetActive(false);
        }
        if (index >= 0 && index < element_hands.Count)
            element_hands[index].SetActive(true);
    }

    /// <summary>
    /// Scrolls through all the unlocked marks on the in-game menu
    /// </summary>
    /// <param name="direction">Direction of the scroll (1 or -1)</param>
    private void ScrollMarksList(int direction)
    {
        if (marks_list.Count <= 1) return;

        current_mark += direction;
        if (current_mark > unlocked_marks.Count - 1) current_mark = 0;
        else if (current_mark < 0) current_mark = unlocked_marks.Count - 1;

        RefreshMarksList();
    }

    /// <summary>
    /// Refreshes the order for the scroll on the marks in-game menu
    /// </summary>
    private void RefreshMarksList()
    {
        int count = unlocked_marks.Count;
        if (count == 0) return;

        for (int i = 0; i < count; i++)
        {
            // Place it around the parent
            float angle = i * Mathf.PI * 2 / count; /// Get the angle where the mark will be
            float x = Mathf.Cos(angle) * 1;
            float y = Mathf.Sin(angle) * 1;

            marks_list_images[(int)unlocked_marks[i]].rectTransform.anchoredPosition = new Vector2(x, y);
            marks_list_images[(int)unlocked_marks[i]].rectTransform.sizeDelta = Vector2.one * (i == current_mark ? 0.6f : 0.5f);
        }

        int index = (int)unlocked_marks[current_mark];

        Element element = marks[index].element;
        equipped_mark = element;
        mark_placeholder_image.sprite = element.im_mark_sprite;

        EnableElementHand(index);
    }

    #region Inputs
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
    public void SelectMarkOnMenu(InputAction.CallbackContext con)
    {
        if (con.performed && marks_canvas.activeSelf)
        {
            PlaceMarkOnSurface();
        }
    }
    public void ActivateMark(InputAction.CallbackContext con)
    {
        if (con.performed)
        {
            ActivateMark();
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