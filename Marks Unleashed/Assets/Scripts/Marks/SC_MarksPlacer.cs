using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.Rendering.DebugUI;

public class SC_MarksPlacer : MonoBehaviour
{
    public static SC_MarksPlacer Instance { get; private set; }

    public string first_rune = "Tierra";

    [Header("References")]
    public GameObject ob_mark_disk;
    public GameObject mark_placeholder;
    public UnityEngine.UI.Image mark_placeholder_image;
    public Transform player;
    public LayerMask layer_placeable;
    public SkinnedMeshRenderer meshRend_arm;

    public Transform element_hands_parent;

    private Rigidbody mark_placeholder_rb;
    private SC_CameraRotation cameraRotation;

    private SC_Grab script_grab = null;

    // Marks activation order UI
    public Transform ui_order_parent;
    private UnityEngine.UI.Image[] im_activation_order;

    private List<GameObject> element_hands = new List<GameObject>();

    public Transform tr_left_arm;
    private Animator _anim_arm;

    // World UI
    public GameObject arrow_gameObject;
    private Transform transform_arrow;

    // Marks selection
    public GameObject marks_canvas;
    private List<GameObject> marks_list = new List<GameObject>();
    private List<ElementTypes> unlocked_marks = new List<ElementTypes>();
    private List<SC_Mark> marks = new List<SC_Mark>();
    private List<Transform> marks_list_images = new List<Transform>();
    public float marks_radius = 10;

    private bool open_menu = false;

    [Header("Sounds")]
    public AudioClip clip_place_mark;
    public List<AudioClip> clips_swhoosh;
    public AudioClip clip_select_rune;
    public AudioClip clip_charge_rune, clip_charge_rune_end;

    private AudioSource _audioSource;

    [Header("Stats")]
    public float range_placing = 10;
    public int max_marks = 3;

    [Header("Marks")]
    public Element equipped_mark;

    public List<SC_Mark> placed_marks;
    private ElementTypes element_last_activated;

    [HideInInspector] public int current_mark = 0;

    public bool placing_mark = false;
    public bool activate_mark = false;

    [HideInInspector] public float disk_launch_force = 75;
    [HideInInspector] public float launch_timer = 0;
    private bool launching_disk = false;
    private bool launch_mode = false;

    private MaterialPropertyBlock _mpb;

    public event Action event_destroy_runes;

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

        _audioSource = GetComponent<AudioSource>();

        transform_arrow = Instantiate(arrow_gameObject).transform;
        transform_arrow.localScale = Vector3.one * 0.01f;
        transform_arrow.gameObject.SetActive(false);

        _mpb = new MaterialPropertyBlock();

        _anim_arm = tr_left_arm.GetComponentInParent<Animator>();
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

            GameObject mark_disk = Instantiate(mark.disk, ob.transform, true);
            mark_disk.transform.position = Vector3.zero;
            mark_disk.transform.localPosition = Vector3.zero;
            mark_disk.transform.localScale = Vector3.one * 0.25f;
            mark_disk.transform.rotation = new Quaternion(0, 0, 0, 0);
            mark_disk.transform.localRotation = new Quaternion(0, 0, 0, 0);

            int child_count = mark_disk.transform.childCount;
            for (int j = 0; j < child_count; j++)
            {
                mark_disk.transform.GetChild(j).gameObject.layer = marks_canvas.layer;
                mark_disk.transform.GetChild(j).rotation = new Quaternion(0, 0, 0, 0);
                mark_disk.transform.GetChild(j).localRotation = new Quaternion(0, 0, 0, 0);
                Debug.Log(mark_disk.transform.GetChild(j).name);
            }
            

            ParticleSystem disk_parts = mark_disk.GetComponentInChildren<ParticleSystem>();
            var emission = disk_parts.emission;
            emission.rateOverTime = 0;
            emission.rateOverDistance = 1;
            emission.burstCount = 0;

            //UnityEngine.UI.Image im = ob.AddComponent<UnityEngine.UI.Image>();
            //im.sprite = mark.element.im_mark_sprite;

            float t = (float)i / (4 - 1);
            float step = marks_count > 1 ? Mathf.PI / (marks_count - 1) : 0f;
            float angle = (i - current_mark) * step + Mathf.PI / 2f;
            float x = Mathf.Cos(angle) * marks_radius;
            float y = Mathf.Sin(angle) * marks_radius;

            if (i == 0)
            {
                x = 0;
                y = 0;
                mark_disk.transform.LookAt(Camera.main.transform);
            }

            ob.transform.localPosition = new Vector3(x, y, 0);

            //RectTransform rect = ob.GetComponent<RectTransform>();
            //rect.sizeDelta = new Vector2(0.5f, 0.5f);
            //rect.anchoredPosition = new Vector2(x, y);

            marks_list.Add(ob);
            marks.Add(mark);
            marks_list_images.Add(ob.transform);

            //cameraRotation.AddUIElementToMove(ob.GetComponent<RectTransform>());
        }

        equipped_mark = marks[current_mark].element;

        RefreshMarksList();

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

    public void ClearUnlockedRunes()
    {
        unlocked_marks.Clear();
    }
    public void RemoveMarks()
    {
        foreach(SC_Mark mark in placed_marks)
        {
            Destroy(mark.gameObject);
        }
        placed_marks.Clear();

        event_destroy_runes?.Invoke();

        current_mark = 0;

        RefreshMarksList();
    }
    public void UnlockMark(ElementTypes type)
    {
        // Don't add an unlocked mark
        if ((int)type >= marks.Count) return;

        if (unlocked_marks.Contains(marks[(int)type].element.el_type)) return;
        unlocked_marks.Add(marks[(int)type].element.el_type);
        marks_list_images[(int)type].gameObject.SetActive(true);

        RefreshMarksList();
    }
    public List<ElementTypes> GetUnlockedMarks()
    {
        return unlocked_marks;
    }

    public ElementTypes GetCurrentRuneType()
    {
        return element_last_activated;
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
    private IEnumerator UpdateMarkUIPosition(Transform tr, Vector2 start_pos, Vector2 end_pos, Quaternion start_rot, Quaternion end_rot, Vector3 start_size, Vector3 end_size)
    {
        float timer = 0;
        float duration = 0.15f;
        while (timer < duration)
        {
            float t = timer / duration;
            t = Mathf.SmoothStep(0, 1, t);
            tr.localPosition = Vector2.Lerp(start_pos, end_pos, t);
            tr.localRotation = Quaternion.Lerp(start_rot, end_rot, t);
            tr.localScale = Vector3.Lerp(start_size, end_size, t);

            timer += Time.deltaTime;
            yield return null;
        }

        tr.localPosition = end_pos;
        tr.rotation = end_rot;
    }
    private IEnumerator FlotateRoutine(Transform tr, float duration, float distance, float start_pos)
    {
        float timer = 0;

        float start_y = start_pos - distance;
        float end_y = start_pos + distance;

        while (timer < duration)
        {
            float t = timer / duration;
            t = Mathf.SmoothStep(0, 1, t);

            Vector3 pos = tr.localPosition;
            tr.localPosition = Vector3.Lerp(new Vector3(pos.x, start_y, pos.z), new Vector3(pos.x, end_y, pos.z), t);

            timer += Time.deltaTime;
            yield return null;
        }
        timer = 0;
        while (timer < duration)
        {
            float t = timer / duration;
            t = Mathf.SmoothStep(0, 1, t);

            Vector3 pos = tr.localPosition;
            tr.localPosition = Vector3.Lerp(new Vector3(pos.x, end_y, pos.z), new Vector3(pos.x, start_y, pos.z), t);

            timer += Time.deltaTime;
            yield return null;
        }

        StartCoroutine(FlotateRoutine(tr, duration, distance, start_pos));
    }
    private IEnumerator RotateRoutine(Transform tr, Quaternion target_rot, float duration)
    {
        float timer = 0;

        Quaternion start_rot = tr.localRotation;

        while (timer < duration)
        {
            yield return new WaitForEndOfFrame();

            float t = timer / duration;
            t = Mathf.SmoothStep(0, 1, t);

            tr.localRotation = Quaternion.Lerp(start_rot, target_rot, t);

            timer += Time.deltaTime;
        }
    }

    /// <summary>
    /// Activates the effects of the placed mark
    /// </summary>
    private void ActivateMark()
    {
        SetArrowPos();

        if (placed_marks.Count <= 0) return;

        StartCoroutine(ActivateMarkAnimation());

        int activate_index = 0;
        // Checks if the first rune is null and searches for the next one
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
        // Activates the rune
        element_last_activated = placed_marks[activate_index].element.el_type;
        placed_marks[activate_index].GetComponent<SC_Mark>().ActivateMark();
        placed_marks.RemoveAt(0);

        switch (element_last_activated)
        {
            default:
                _anim_arm.Play("anim_toon_arm_activate_rune", 0, 0);
                break;
            case ElementTypes.STONE:
                _anim_arm.Play("anim_toon_arm_activate_rune_stone", 0, 0);
                break;
        }
        

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

        GameObject mark_disk = new GameObject($"Mark {mark_info.element.name} disk {UnityEngine.Random.Range(10, 999)}");
        mark_disk.transform.position = cameraRotation.transform.position + cameraRotation.transform.forward * 2;
        mark_disk.layer = LayerMask.NameToLayer("RuneDisk");

        SC_MarkDisk disk = mark_disk.AddComponent<SC_MarkDisk>();
        disk.InitDisk(this, mark_info, Camera.main.transform.forward, disk_launch_force * timer);

        SC_SoundManager.instance.InstantiateSound(clips_swhoosh[UnityEngine.Random.Range(0, clips_swhoosh.Count)], transform.position);
        cameraRotation.ShakeCamera(0.25f, 0.05f);

        placing_mark = false;
    }
    /// <summary>
    /// Instantiates a mark on the looking surface and adds it to the placed marks list
    /// </summary>
    private void PlaceMarkOnSurface(float timer)
    {
        if (!open_menu || !placing_mark || unlocked_marks.Count <= 0) return;

        OpenMarkSelectionMenu();
        PlaceMark((int)unlocked_marks[current_mark], timer);
    }

    #region SelectionMenu
    /// <summary>
    /// Opens or closes the mark in-game selection menu
    /// </summary>
    private void OpenMarkSelectionMenu()
    {
        if (unlocked_marks.Count <= 0) return;
        if (SC_GameManager.instance.first_play)
        {
            SC_GameManager.instance.ShowTutorial(first_rune, 1);
            SC_GameManager.instance.ShowText("Hold left click to shoot");
            SC_GameManager.instance.first_play = false;
        }

        open_menu = !open_menu;
        //_anim_arm.enabled = !open_menu;
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

        

        RefreshMarksList();
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
        if (unlocked_marks.Count <= 1) return;

        //current_mark = Mathf.Clamp(current_mark + direction, 0, unlocked_marks.Count - 1);
        current_mark += direction;
        if (current_mark > unlocked_marks.Count - 1)
            current_mark = 0;
        else if (current_mark < 0)
            current_mark = unlocked_marks.Count - 1;

        _audioSource.PlayOneShot(clip_select_rune);

        RefreshMarksList();
    }

    /// <summary>
    /// Refreshes the order for the scroll on the marks in-game menu
    /// </summary>
    private void RefreshMarksList()
    {
        current_mark = Mathf.Clamp(current_mark, 0, Mathf.Max(0, marks_list_images.Count - 1));

        int rune_count = unlocked_marks.Count;
        if (rune_count == 0) return;

        StopAllCoroutines();

        // Handle single mark edge case
        float step = rune_count > 1 ? Mathf.PI / (rune_count - 1) : 0f;

        for (int i = 0; i < rune_count; i++)
        {
            int indx = (int)unlocked_marks[i];

            // Validate indexs
            if (i >= marks_list_images.Count || indx >= marks_list_images.Count) break;
            if (current_mark >= rune_count) break;

            float angle = (i - current_mark) * step + Mathf.PI / 2f;
            float x = Mathf.Cos(angle) * marks_radius;
            float y = Mathf.Sin(angle) * marks_radius;

            // Makes the selected rune look at the camera
            Quaternion target_rot;
            Vector3 new_scale = Vector3.one * 0.75f;
            if (i == current_mark)
            {
                x = 0;
                y = 0;

                Vector3 dir_to_camera = ((marks_canvas.transform.position - marks_canvas.transform.forward * 2)
                    - marks_list_images[i].position).normalized;
                target_rot = Quaternion.LookRotation(dir_to_camera);

                new_scale = Vector3.one * 3;

                // Use indx instead of i to get the correct remapped mark
                MeshRenderer meshr_disk = marks_list_images[indx].gameObject.GetComponentInChildren<MeshRenderer>();

                // Use the material instance, not shared
                Color col = GetOutlineColor(meshr_disk);
                SetOutlineColor(col);

                
            }
            else
            {
                Vector3 local_dir = (marks_list_images[current_mark].transform.localPosition
                    - marks_list_images[i].transform.localPosition).normalized;
                target_rot = local_dir != Vector3.zero
                    ? Quaternion.LookRotation(local_dir)
                    : Quaternion.identity;
            }

            // Set the rune position
            StartCoroutine(UpdateMarkUIPosition(
                marks_list_images[indx].transform,
                marks_list_images[indx].transform.localPosition,
                new Vector2(x, y),
                marks_list_images[indx].transform.localRotation,
                target_rot,
                marks_list_images[indx].transform.localScale,
                new_scale
            ));
        }

        int index = (int)unlocked_marks[current_mark];
        StartCoroutine(FlotateRoutine(marks_list_images[index].transform, 1, 0.05f, 0));

        StartCoroutine(RotateRoutine(tr_left_arm, Quaternion.Euler(0, (225 / unlocked_marks.Count) * current_mark, 0), 0.25f));

        Element element = marks[index].element;
        equipped_mark = element;
        mark_placeholder_image.sprite = element.im_mark_sprite;
        EnableElementHand(index);

        
    }
    private Color GetOutlineColor(Renderer renderer)
    {
        // Always use material instance, never sharedMaterial
        Material mat = renderer.material;

        if (mat == null) return Color.black;

        if (mat.HasProperty("_Outline_Color"))
            return mat.GetColor("_Outline_Color");

        if (mat.HasProperty("_BaseColor"))
            return mat.GetColor("_BaseColor");

        return Color.black;
    }
    public void SetOutlineColor(Color color)
    {
        if (meshRend_arm == null) return;
        meshRend_arm.GetPropertyBlock(_mpb);
        _mpb.SetColor("_Outline_Color", color);
        meshRend_arm.SetPropertyBlock(_mpb);
    }

    private IEnumerator PlayChargeSoundRoutine(float duration)
    {
        float timer = 0;
        float timer_sound = 0;

        SC_SoundManager.instance.InstantiateSound(clip_charge_rune, transform.position);
        /*
        while (timer < duration * 0.95f && launching_disk)
        {
            if (timer_sound > duration * 0.1f)
            {
                float t = timer / duration;
                //SC_SoundManager.instance.InstantiateSound(clip_charge_rune, transform.position, false, 0.5f + t);
                timer_sound = 0;
            }

            timer_sound += Time.deltaTime;
            timer += Time.deltaTime;
            yield return null;
        }*/
        yield return new WaitForSeconds(duration * 0.95f);
        SC_SoundManager.instance.InstantiateSound(clip_charge_rune_end, transform.position);
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
                cameraRotation.OffsetCamera(-Camera.main.transform.forward, 0.5f, 1, true);
                cameraRotation.ShakeCameraProgressive(0.05f, 0.2f, 1.5f);
                cameraRotation.ShakeUIElements(10, 1.5f);
                StartCoroutine(PlayChargeSoundRoutine(0.95f));
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
                cameraRotation.OffsetCameraAndReturn(Camera.main.transform.forward, -0.5f, Quaternion.identity, 0.5f, true, true);
                

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
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;

        Gizmos.DrawWireSphere(marks_canvas.transform.position - marks_canvas.transform.forward * 2, 0.1f);
    }
}