using UnityEngine;

/// <summary>
/// Class dedicated to play coroutines
/// </summary>
public class SC_CoroutineRunner : MonoBehaviour
{
    public static SC_CoroutineRunner instance {  get; private set; }
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
}