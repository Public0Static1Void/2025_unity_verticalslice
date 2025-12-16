using UnityEngine;

public class SC_Interactable : MonoBehaviour, IN_Interactable
{
    public virtual void Activate() { }
    public virtual void Deactivate() { }
}
public interface IN_Interactable
{
    void Activate();
    void Deactivate();
}