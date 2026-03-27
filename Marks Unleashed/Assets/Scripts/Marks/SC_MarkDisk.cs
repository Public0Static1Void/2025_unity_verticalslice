using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class SC_MarkDisk : MonoBehaviour
{
    public SC_Mark stored_mark;
    private SC_MarksPlacer marksPlacer;
    private Transform rune_disk;
    private Rigidbody rb;
    private GameObject ob_disk;

    private Vector3 move_dir;
    private Vector3 previous_pos;

    private bool activated = false;

    private void Awake()
    {
        rb = gameObject.AddComponent<Rigidbody>();
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
    }

    private void Start()
    {
        Destroy(gameObject, 10);
        gameObject.AddComponent<BoxCollider>();
    }

    public void InitDisk(SC_MarksPlacer marks_placer, SC_Mark place_mark, Vector3 direction, float speed)
    {
        marksPlacer = marks_placer;
        stored_mark = place_mark;
        move_dir = direction;

        ob_disk = Instantiate(stored_mark.disk, transform.position, transform.rotation);
        ob_disk.transform.SetParent(transform, false);
        ob_disk.transform.localPosition = Vector3.zero;

        rune_disk = ob_disk.transform;

        // Adds force to the rigidbody
        rb.AddForce(direction * speed, ForceMode.Impulse);
    }

    private void Update()
    {
        // Rotate the disk with the current movement direction
        Vector3 dir = (transform.position - previous_pos).normalized;
        if (dir.sqrMagnitude > 0.0001f)
            rune_disk.transform.rotation = Quaternion.LookRotation(dir);

        /// Store the last frame position
        previous_pos = transform.position;
    }

    private bool CheckNearbySurface(Vector3 dir)
    {
        if (Physics.Raycast(transform.position, dir, out RaycastHit hit, 1))
        {
            // Get the position and rotation
            Vector3 position = hit.point + hit.normal * 0.1f;
            Quaternion rotation = Quaternion.LookRotation(hit.normal, move_dir);


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

    private void OnCollisionEnter(Collision collision)
    {
        if (activated) return;

        if (collision.gameObject.name == "Player")
        {
            collision.gameObject.GetComponent<SC_PlayerLife>().Damage(1);
        }

        Vector3 dir_to_surface = collision.GetContact(0).normal;

        if (CheckNearbySurface(-dir_to_surface))
        {
            ParticleSystem parts = ob_disk.GetComponentInChildren<ParticleSystem>();
            parts.Stop();
            parts.transform.parent = null;

            activated = true;
            Destroy(gameObject);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 1);
    }
}