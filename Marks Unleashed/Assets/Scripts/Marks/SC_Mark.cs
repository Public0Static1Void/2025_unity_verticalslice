using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody))]
public class SC_Mark : MonoBehaviour
{
    public ElementTypes type = ElementTypes.NONE_LAST;
    public Element element;
    public GameObject particles;

    public LayerMask layer_interactable = ~0;

    public bool enable_collision_activation = false;

    public bool regenerate = false;

    private Rigidbody rb;
    private Image mark_image;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        mark_image = GetComponentInChildren<Image>();

        rb.useGravity = false;
        rb.isKinematic = true;
    }

    public void Init(Element el, GameObject parts = null)
    {
        element = el;
        type = el.el_type;
        particles = parts;
    }
    public void EnableCollision()
    {
        SphereCollider coll = gameObject.AddComponent<SphereCollider>();
        coll.isTrigger = true;
        coll.radius = 1;

        enable_collision_activation = true;
    }

    // Mark activation
    public virtual void Activate()
    {
        Debug.Log("Mark activated");
    }
    public void DesactivateMark()
    {
        StartCoroutine(DesintegrateMark());
    }
    private void OnDestroy()
    {
        if (transform.parent != null)
        {
            Destroy(transform.parent.gameObject);
            SC_MarksPlacer.Instance.RemoveMarkFromPlaced(gameObject.name);
        }
    }

    private IEnumerator DesintegrateMark()
    {
        if (mark_image == null)
        {
            if (element == null)
            {
                GetElement();
            }
            mark_image = GetComponentInChildren<Image>();
        }
        float alpha = mark_image.color.a;
        while (alpha > 0)
        {
            alpha -= Time.deltaTime * 4;

            Color col = mark_image.color;
            mark_image.color = new Color(col.r, col.g, col.b, alpha);

            yield return null;
        }

        if (regenerate)
        {
            yield return new WaitForSeconds(0.5f);
            StartCoroutine(ReapearMark());
        }
        else
        {
            Destroy(gameObject);
        }
    }
    private IEnumerator ReapearMark()
    {
        if (mark_image == null)
        {
            if (element == null)
            {
                GetElement();
            }
            mark_image = GetComponentInChildren<Image>();
        }
        float alpha = mark_image.color.a;
        while (alpha < 1)
        {
            alpha += Time.deltaTime * 8;

            Color col = mark_image.color;
            mark_image.color = new Color(col.r, col.g, col.b, alpha);

            yield return null;
        }
    }

    // Mark movement
    public void LaunchMark(Vector3 force)
    {
        rb.isKinematic = false;
        rb.AddForce(force);
    }

    // Utility
    public Element GetElement()
    {
        Element ret_element = null;
        if (element == null)
        {
            try
            {
                element = SC_ElementManager.instance.GetElement(type);
                ret_element = element;
            }
            catch { }
        }

        return ret_element;
    }
    public int GetElementStrenght()
    {
        int str = 1;
        
        if (element == null)
        {
            try
            {
                element = SC_ElementManager.instance.GetElement(type);
            } catch { }
        }
        else
        {
            str = element.strength;
        }

        return str;
    }

    

    public void OnTriggerEnter(Collider other)
    {
        if (!enable_collision_activation) return;

        if (rb.linearVelocity.magnitude >= 1)
        {
            Activate();
            return;
        }

        Rigidbody rb_other = other.GetComponent<Rigidbody>();
        if (rb_other != null)
        {
            Debug.Log(rb_other.linearVelocity.magnitude);
            if (rb_other.linearVelocity.magnitude >= 1.5f)
                Activate();
        }
    }
}