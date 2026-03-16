using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions.Must;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.UIElements;
using UnityEngine.XR;

public class SC_MarksPlacer : MonoBehaviour
{
    public static SC_MarksPlacer Instance { get; private set; }
    [Header("References")]
    public GameObject ob_mark_disk;
    public GameObject mark_placeholder;
    public UnityEngine.UI.Image mark_placeholder_image;
    public Transform player;
    public LayerMask layer_placeable;

    public Transform element_hands_parent;

    private Rigidbody mark_placeholder_rb;
    private SC_CameraRotation cameraRotation;

    private SC_Grab script_grab = null;

    // Marks activation order UI
    public Transform ui_order_parent;
    private UnityEngine.UI.Image[] im_activation_order;

    private List<GameObject> element_hands = new List<GameObject>();

    // World UI
    public GameObject arrow_gameObject;
    private Transform transform_arrow;

    // Marks selection
    public GameObject marks_canvas;
    private List<GameObject> marks_list = new List<GameObject>();
    private List<ElementTypes> unlocked_marks = new List<ElementTypes>();
    private List<SC_Mark> marks = new List<SC_Mark>();
    private List<UnityEngine.UI.Image> marks_list_images = new List<UnityEngine.UI.Image>();
    public float marks_radius = 10;

    private bool open_menu = false;

    [Header("Sounds")]
    public AudioClip clip_place_mark;
    public List<AudioClip> clips_swhoosh;

    [Header("Stats")]
    public float range_placing = 10;
    public int max_marks = 3;

    [Header("Marks")]
    public Element equipped_mark;

    public List<SC_Mark> placed_marks;

    private int current_mark = 0;

    public bool placing_mark = false;
    public bool activate_mark = false;


    private float launch_timer = 0;
    private bool launching_disk = false;
    private bool launch_mode = false;


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    private void Start()
    {
        StartCoroutine(InitMarksUI());

        mark_placeholder = Instantiate(mark_placeholder, player.transform.position, Quaternion.identity);
        marks_canvas.SetActive(false);
        mark_placeholder.SetActive(false);
        mark_placeholder.GetComponentInChildren<Canvas>().sortingOrder = -2;
        mark_placeholder_rb =  mark_placeholder.GetComponent<Rigidbody>();
        mark_placeholder_rb.isKinematic = true;

        mark_placeholder_image = mark_placeholder.GetComponentInChildren<UnityEngine.UI.Image>();

        // Marks activation order UI
        im_activation_order = SC_GameManager.GetChildsComponents<UnityEngine.UI.Image>(ui_order_parent);

        // Get the camera script
        cameraRotation = GetComponentInChildren<SC_CameraRotation>();

        // Get the grab script
        script_grab = GetComponent<SC_Grab>();

        transform_arrow = Instantiate(arrow_gameObject).transform;
        transform_arrow.localScale = Vector3.one * 0.01f;
        transform_arrow.gameObject.SetActive(false);
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

            UnityEngine.UI.Image im = ob.AddComponent<UnityEngine.UI.Image>();
            im.sprite = mark.element.im_mark_sprite;

            float t = (float)i / (4 - 1);
            float anngle = t * Mathf.PI;

            float x = Mathf.Cos(anngle + 45) * marks_radius;
            float y = Mathf.Sin(anngle + 45) * marks_radius;

            ob.transform.localPosition = new Vector3(x, y, 0);



            RectTransform rect = ob.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(x, y);

            marks_list.Add(ob);
            marks.Add(mark);
            marks_list_images.Add(im);
        }

        equipped_mark = marks[current_mark].element;

        RefreshMarksList(0);

        // Load the element hands
        for (int i = 0; i < marks_list.Count; i++)
        {
            if (element_hands_parent.GetChild(i) != null)
                element_hands.Add(element_hands_parent.GetChild(i).gameObject);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            launch_mode = !launch_mode;
        }
        if (Input.GetKeyDown(KeyCode.F))
        {
            UnlockMark(marks[(int)ElementTypes.AIR].element.el_type);
            UnlockMark(marks[(int)ElementTypes.FIRE].element.el_type);
            UnlockMark(marks[(int)ElementTypes.STONE].element.el_type);
            UnlockMark(marks[(int)ElementTypes.ICE].element.el_type);
        }
        if (placing_mark)
        {
            //Horizontal del jugador
            Vector3 new_pos;
            if (Physics.Raycast(player.position, Camera.main.transform.forward, out RaycastHit hit, range_placing, layer_placeable, QueryTriggerInteraction.Ignore))
            {
                new_pos = hit.point;

                mark_placeholder.transform.rotation = Quaternion.LookRotation(hit.normal, transform.forward);
                mark_placeholder_image.color = Color.white;
            }
            else
            {
                new_pos = player.position + Camera.main.transform.forward * range_placing;
                mark_placeholder_image.color = Color.black;
                mark_placeholder.transform.LookAt(Camera.main.transform);
            }
            // Moves the mark to the camera, overlapping with collisions
            mark_placeholder.transform.position = Vector3.Lerp(mark_placeholder.transform.position, new_pos, Time.deltaTime * 30);

        }
        // Make the activatios order arrow look to the player
        if (placed_marks.Count > 0)
        {
            transform_arrow.LookAt(transform);
        }

        if (launching_disk)
        {
            launch_timer += Time.deltaTime;
        }
    }

    public void UnlockMark(ElementTypes type)
    {
        unlocked_marks.Add(marks[(int)type].element.el_type);
        marks_list_images[(int)type].gameObject.SetActive(true);
    }
    public List<ElementTypes> GetUnlockedMarks()
    {
        return unlocked_marks;
    }

    public void RemoveMarkFromPlaced(string mark_name)
    {
        for (int i = 0; i < placed_marks.Count; i++)
        {
            if (placed_marks[i].name == mark_name)
            {
                placed_marks.RemoveAt(i);
                break;
            }
        }

        UpdateActivateMarksUI();
    }

    public void UpdateActivateMarksUI()
    {
        for (int i = 0; i < im_activation_order.Length; i++)
        {
            if (im_activation_order[i] == null) continue;

            Sprite spr = null;
            if (i < placed_marks.Count && placed_marks[i] != null)
            {
                spr = placed_marks[i].element.im_mark_sprite;
                im_activation_order[i].gameObject.SetActive (true);
            }
            else
            {
                if (im_activation_order[i] != null)
                    im_activation_order[i].gameObject.SetActive(false);
            }

            if (im_activation_order[i] != null)
                im_activation_order[i].sprite = spr;
        }  
    }
    private IEnumerator UpdateMarkUIPosition(Transform tr, Vector2 start_pos, Vector2 end_pos, Quaternion start_rot, Quaternion end_rot)
    {
        float t = 0;
        while (t < 1)
        {
            tr.localPosition = Vector2.Lerp(start_pos, end_pos, t);
            tr.rotation = Quaternion.Lerp(start_rot, end_rot, t);
            t += Time.deltaTime * 10;
            yield return null;
        }

        tr.localPosition = end_pos;
        tr.rotation = end_rot;
    }

    /// <summary>
    /// Activates the effects of all the placed marks
    /// </summary>
    private void ActivateMark()
    {
        if (placed_marks.Count <= 0) return;

        StartCoroutine(ActivateMarkAnimation());

        int activate_index = 0;
        if (placed_marks[0] == null)
        {
            placed_marks.RemoveAt(0);
            for (int i = 0; i < placed_marks.Count; i++)
            {
                if (placed_marks[i] != null)
                {
                    activate_index = i;
                    break;
                }
                else
                {
                    UpdateActivateMarksUI();
                    return;
                }
            }
        }

        placed_marks[activate_index].GetComponent<SC_Mark>().ActivateMark();
        placed_marks.RemoveAt(0);

        SetArrowPos();

        UpdateActivateMarksUI();
    }

    private IEnumerator ActivateMarkAnimation()
    {
        activate_mark = true;
        yield return new WaitForSeconds(0.1f);
        activate_mark = false;
    }

    public void SetArrowPos()
    {
        if (placed_marks.Count <= 0)
        {
            transform_arrow.gameObject.SetActive(false);
            return;
        }
        // Set the arrow position
        transform_arrow.position = placed_marks[0].transform.position + placed_marks[0].transform.forward * 1.15f;
        transform_arrow.gameObject.SetActive(true);
    }

    private void PlaceMark(int index, float timer)
    {
        if (timer > 1) timer = 1;
        // Get the selected mark info
        SC_Mark mark_info = SC_ElementManager.instance.marks[index];

        GameObject mark_disk = new GameObject($"Mark {mark_info.element.name} disk");
        mark_disk.transform.position = transform.position + transform.forward * 2;

        SC_MarkDisk disk = mark_disk.AddComponent<SC_MarkDisk>();
        disk.InitDisk(this, mark_info, Camera.main.transform.forward, 75 * timer);

        SC_SoundManager.instance.InstantiateSound(clips_swhoosh[UnityEngine.Random.Range(0, clips_swhoosh.Count)], transform.position);
        cameraRotation.ShakeCamera(0.25f, 0.05f);

        placing_mark = false;
        /*
        // Create canvas parent
        GameObject mark_canvas = new GameObject($"Mark {mark_info.element.name} canvas");
        mark_canvas.transform.position = position;
        mark_canvas.transform.rotation = rotation;

        mark_canvas.transform.SetParent(hit_transform, true);

        mark_canvas.transform.position -= mark_canvas.transform.forward * 200; // Adds offset from the ground
        Canvas canvas = mark_canvas.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;

        // Resize the canvas
        RectTransform rt = canvas.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(0.5f, 0.5f);
        rt.localScale = Vector3.one;

        // Create the UnityEngine.UI.Image
        GameObject inst = new GameObject($"Mark {mark_info.element.name} {placed_marks.Count}");

        inst.transform.position = position;
        inst.transform.rotation = rotation;

        UnityEngine.UI.UnityEngine.UI.Image im = inst.AddComponent<UnityEngine.UI.UnityEngine.UI.Image>();
        im.transform.SetParent(canvas.transform);

        im.sprite = mark_info.element.im_mark_sprite;
        im.rectTransform.anchoredPosition = Vector2.zero;
        im.rectTransform.sizeDelta = Vector2.one * 0.5f;

        // Adds the mark element component
        inst.AddComponent(mark_info.GetType());

        SC_Mark mark = inst.GetComponent(mark_info.GetType()) as SC_Mark;
        mark.particles = mark_info.particles;
        mark.element = mark_info.element;
        mark.enable_collision_activation = true;
        mark.clip_activation = mark_info.clip_activation;

        BoxCollider coll = mark.gameObject.AddComponent<BoxCollider>();
        coll.isTrigger = true;

        SC_SoundManager.instance.InstantiateSound(clip_place_mark, position);

        placed_marks.Add(mark);
        */
    }
    /// <summary>
    /// Instantiates a mark on the looking surface and adds it to the placed marks list
    /// </summary>
    private void PlaceMarkOnSurface(float timer)
    {
        if (!open_menu || !placing_mark || unlocked_marks.Count <= 0) return;

        OpenMarkSelectionMenu();
        PlaceMark((int)unlocked_marks[current_mark], timer);
        /*
        if (Physics.Raycast(player.position, Camera.main.transform.forward, out RaycastHit hit, range_placing, layer_placeable, QueryTriggerInteraction.Ignore))
        {
            Vector3 position = hit.point + (hit.normal * 0.1f);

            OpenMarkSelectionMenu();
            // Places the mark on the look point
            PlaceMark((int)unlocked_marks[current_mark], hit.transform, position, Quaternion.LookRotation(hit.normal, transform.forward));
        }
        */
    }

    #region SelectionMenu
    /// <summary>
    /// Opens or closes the mark in-game selection menu
    /// </summary>
    private void OpenMarkSelectionMenu()
    {
        if (unlocked_marks.Count <= 0) return;

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
            cameraRotation.SetTurnSpeed(0.75f);
            placing_mark = true;
        }
        else
        {
            cameraRotation.SetTurnSpeed(1);
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
        if (current_mark > unlocked_marks.Count - 1) current_mark = unlocked_marks.Count - 1;
        else if (current_mark < 0) current_mark = 0;

        RefreshMarksList(direction);
    }

    /// <summary>
    /// Refreshes the order for the scroll on the marks in-game menu
    /// </summary>
    private void RefreshMarksList(int dir)
    {
        /*
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
        */

        int rune_count = unlocked_marks.Count;
        if (rune_count == 0) return;

        //float angle = (180 / rune_count) * current_mark;
        //marks_canvas.transform.localRotation = Quaternion.Euler(0, 0, angle);

        StopCoroutine("UpdateMarkUIPosition");
        float step = Mathf.PI / (rune_count - 1);
        for (int i = 0; i < rune_count; i++)
        {
            if (i >= marks_list_images.Count) break;

            int indx = (int)unlocked_marks[i];

            float angle = (i - current_mark) * step + Mathf.PI / 2;

            float x = Mathf.Cos(angle) * marks_radius;
            float y = Mathf.Sin(angle) * marks_radius;

            Vector3 local_dir = (marks_list_images[current_mark].transform.localPosition - marks_list_images[i].transform.localPosition).normalized;
            Quaternion target_rot = Quaternion.LookRotation(local_dir);
            if (i == current_mark)
            {
                Vector3 dir_to_camera = (Camera.main.transform.position - marks_list_images[i].transform.position).normalized;
                target_rot = Quaternion.LookRotation(dir_to_camera);
            }

            StartCoroutine(UpdateMarkUIPosition(
                marks_list_images[indx].transform,
                marks_list_images[indx].transform.localPosition,
                new Vector2(x, y),
                marks_list_images[indx].transform.localRotation,
                target_rot
            ));
            //marks_list_images[indx].rectTransform.anchoredPosition = new Vector2(x, y);
            //marks_list_images[indx].rectTransform.sizeDelta = Vector2.one * (i == current_mark ? 0.6f : 0.5f);
        }

        int index = (int)unlocked_marks[current_mark];

        Element element = marks[index].element;
        equipped_mark = element;
        mark_placeholder_image.sprite = element.im_mark_sprite;

        EnableElementHand(index);
    }

    #endregion

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
        if (con.performed && marks_canvas.activeSelf
            && placed_marks.Count < max_marks)
        {
            if (!launch_mode)
            {
                launching_disk = true;
                cameraRotation.ChangeFov(70, 1.5f);
            }
            else
            {
                PlaceMarkOnSurface(1);
            }
        }
        if (con.canceled && launch_timer > 0)
        {
            if (!launch_mode)
            {
                cameraRotation.ChangeFov(cameraRotation.base_fov, 0.25f);

                launching_disk = false;

                PlaceMarkOnSurface(launch_timer);
                launch_timer = 0;
            }
        }
    }
    public void ActivateMark(InputAction.CallbackContext con)
    {
        if (con.performed && !script_grab.grabbing) // Can't activate the marks while grabbing an object
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