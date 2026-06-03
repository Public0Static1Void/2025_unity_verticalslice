using UnityEngine;
using UnityEngine.SceneManagement;

public class SC_CheckpointManager : MonoBehaviour
{
    public Vector3 currentCheckpointPos; //Almacena último checkpoint alcanzado
    public bool hasCheckpoint = false; //El jugador ha tocado algún checkpoint
    
    public static SC_CheckpointManager Instance;
    
    private void Awake()
    {
        //Singleton
        if (Instance == null) 
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    #region Reset Checkpoint On New Scene Loaded
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        hasCheckpoint = false;
    }
    #endregion
    public void SetCheckpoint(Vector3 newChackpointPos) //Funcion que se encarga de cambiar checkpoint del player
    {
        currentCheckpointPos = newChackpointPos;
        hasCheckpoint = true;
    }
}
