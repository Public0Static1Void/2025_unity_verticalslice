using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEditor.Profiling.FrameDataView;

public class SC_MarkDisk : MonoBehaviour
{
    public SC_Mark stored_mark;

    private SC_MarksPlacer marksPlacer;
    private Vector3 move_dir;
    private float move_speed;

    // Update is called once per frame
    void Update()
    {
        transform.Translate(move_dir * move_speed * Time.deltaTime);
    }
    private void FixedUpdate()
    {
        Collider[] colls = Physics.OverlapSphere(transform.position, 1);
        foreach (Collider coll in colls)
        {
            if (coll.name == "Player") continue;

            Vector3 dir_to_nearest = (coll.transform.position - transform.position).normalized;
            if (CheckNearbySurface(dir_to_nearest))
                Destroy(gameObject);
        }
    }

    private bool CheckNearbySurface(Vector3 dir)
    {
        if (Physics.Raycast(transform.position, dir, out RaycastHit hit, 1))
        {
            Vector3 position = hit.point;
            Quaternion rotation = Quaternion.LookRotation(hit.normal);

            GameObject mark_canvas = new GameObject($"Mark {stored_mark.element.name} canvas");
            mark_canvas.transform.position = position;
            mark_canvas.transform.rotation = rotation;

            mark_canvas.transform.SetParent(hit.transform, true);

            mark_canvas.transform.position -= mark_canvas.transform.forward * 200; // Adds offset from the ground
            Canvas canvas = mark_canvas.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;

            // Resize the canvas
            RectTransform rt = canvas.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(0.5f, 0.5f);
            rt.localScale = Vector3.one;

            // Create the image 
            GameObject inst = new GameObject($"Mark {stored_mark.element.name} {marksPlacer.placed_marks.Count}");

            inst.transform.position = position;
            inst.transform.rotation = rotation;

            UnityEngine.UI.Image im = inst.AddComponent<UnityEngine.UI.Image>();
            im.transform.SetParent(canvas.transform);

            im.sprite = stored_mark.element.im_mark_sprite;
            im.rectTransform.anchoredPosition = Vector2.zero;
            im.rectTransform.sizeDelta = Vector2.one * 0.5f;

            // Adds the mark element component
            inst.AddComponent(stored_mark.GetType());

            SC_Mark mark = inst.GetComponent(stored_mark.GetType()) as SC_Mark;
            mark.particles = stored_mark.particles;
            mark.element = stored_mark.element;
            mark.enable_collision_activation = true;
            mark.clip_activation = stored_mark.clip_activation;

            BoxCollider coll = mark.gameObject.AddComponent<BoxCollider>();
            coll.isTrigger = true;

            marksPlacer.placed_marks.Add(mark);
            marksPlacer.SetArrowPos();
            marksPlacer.UpdateActivateMarksUI();

            return true;
        }
        return false;
    }

    public void InitDisk(SC_MarksPlacer marks_placer, SC_Mark place_mark, Vector3 direction, float speed)
    {
        marksPlacer = marks_placer;
        stored_mark = place_mark;
        move_dir = direction;
        move_speed = speed;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 1);
    }
}