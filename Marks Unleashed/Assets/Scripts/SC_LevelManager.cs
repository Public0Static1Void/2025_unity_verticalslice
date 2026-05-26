using UnityEngine;

public class SC_LevelManager : MonoBehaviour
{
    public string load_scene1 = "";
    public string load_scene2 = "";
    public string load_scene3 = "";
    public string load_scene4 = "";
    public string load_scene5 = "";

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            SC_GameManager.instance.LoadSceneAsync(load_scene1);
        }
        else if (Input.GetKeyDown(KeyCode.F2))
        {
            SC_GameManager.instance.LoadSceneAsync(load_scene2);
        }
        else if (Input.GetKeyDown(KeyCode.F3))
        {
            SC_GameManager.instance.LoadSceneAsync(load_scene3);
        }
        else if (Input.GetKeyDown(KeyCode.F4))
        {
            SC_GameManager.instance.LoadSceneAsync(load_scene4);
        }
        else if (Input.GetKeyDown(KeyCode.F5))
        {
            SC_GameManager.instance.LoadSceneAsync(load_scene5);
        }
    }
}
