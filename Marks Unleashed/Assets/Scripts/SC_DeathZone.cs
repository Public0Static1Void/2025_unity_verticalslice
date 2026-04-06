using UnityEngine;
using UnityEngine.SceneManagement;

public class SC_DeathZone : MonoBehaviour
{
    public string load_scene = "SampleScene";
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            SC_GameManager.instance.LoadSceneAsync(load_scene);
        }
        else
        {
            Destroy(other.gameObject);
        }
    }
}