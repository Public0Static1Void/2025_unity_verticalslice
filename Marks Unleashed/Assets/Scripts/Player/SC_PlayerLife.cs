using UnityEngine;
using UnityEngine.SceneManagement;

public class SC_PlayerLife : SC_LifeSystem
{
    public string reload_scene = "SampleScene";
    public override void Die()
    {
        Debug.Log("Dead x)");
        SceneManager.LoadScene(reload_scene);
    }
}