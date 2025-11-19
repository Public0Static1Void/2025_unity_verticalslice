using UnityEngine;

public class SC_Mark : MonoBehaviour
{
    public Element element;

    public virtual void Activate()
    {
        Debug.Log("Mark activated");
        // Mark logic here
    }
}