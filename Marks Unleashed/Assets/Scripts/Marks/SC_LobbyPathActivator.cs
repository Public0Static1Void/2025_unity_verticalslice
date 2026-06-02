using System.Collections;
using UnityEngine;

public class SC_LobbyPathActivator : MonoBehaviour
{
    private int unlocked_num;

    [Header("Paths")]
    public GameObject path_stone;
    public GameObject path_ice;
    public GameObject path_air;
    public GameObject path_final;

    [Header("Doors")]
    public GameObject door_stone;
    public GameObject door_ice;
    public GameObject door_air;
    public GameObject door_final;

    public Transform spawn_pos;

    void Start()
    {
        StartCoroutine(LateStart());
    }

    private IEnumerator LateStart()
    {
        yield return new WaitForSeconds(4);
        unlocked_num = SC_MarksPlacer.Instance.GetUnlockedMarks().Count;

        UnlockElementGameObjects(unlocked_num);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.N))
            UnlockElementGameObjects(4);
    }

    private void UnlockElementGameObjects(int index)
    {
        switch (index)
        {
            default:
                path_stone.SetActive(true);
                path_air.SetActive(true);
                path_final.SetActive(true);
                path_ice.SetActive(true);
                door_stone.SetActive(false);
                door_air.SetActive(false);
                door_final.SetActive(false);
                door_ice.SetActive(false);
                break;
            case 0:
                path_stone.SetActive(true);
                door_stone.SetActive(false);
                break;
            case 2:
                path_air.SetActive(true);
                door_air.SetActive(false);
                break;
            case 1:
                path_ice.SetActive(true);
                door_ice.SetActive(false);
                break;
            case 3:
                path_final.SetActive(true);
                door_final.SetActive(false);
                break;
        }

    }
}
