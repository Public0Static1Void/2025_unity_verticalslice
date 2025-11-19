using UnityEngine;
using UnityEngine.UI;

public class SC_Mark : MonoBehaviour
{
    public Element element;
    public Image image;

    public virtual void Activate()
    {
        Debug.Log("Mark activated");
        // Mark logic here
    }
}