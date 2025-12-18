using UnityEngine;
using UnityEngine.Rendering;

public class SC_Teleporter : MonoBehaviour, IN_Interactable
{
    public string load_scene = "";
    public void Activate()
    {
        SC_GameManager.instance.LoadSceneAsync(load_scene);
    }
    public void Deactivate()
    {

    }
}