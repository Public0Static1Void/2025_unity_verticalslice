using UnityEngine;
using UnityEngine.UI;

public class SC_Mark : MonoBehaviour
{
    public Element element;
    public GameObject particles;

    public LayerMask layer_interactable = ~0;

    public void Init(Element el, GameObject parts)
    {
        element = el;
        particles = parts;
    }
    public virtual void Activate()
    {
        Debug.Log("Mark activated");
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, 3);
    }
}