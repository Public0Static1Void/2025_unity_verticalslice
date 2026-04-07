using UnityEngine;

public class SC_LevelManager : MonoBehaviour
{
    public string load_scene1 = "";
    public string load_scene2 = "";
    public string load_scene3 = "";
    public string load_scene4 = "";

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            SC_GameManager.instance.LoadSceneAsync(load_scene1);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            SC_GameManager.instance.LoadSceneAsync(load_scene2);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            SC_GameManager.instance.LoadSceneAsync(load_scene3);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            SC_GameManager.instance.LoadSceneAsync(load_scene4);
        }
    }
}
