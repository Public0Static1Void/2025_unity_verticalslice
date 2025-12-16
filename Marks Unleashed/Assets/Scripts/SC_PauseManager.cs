using UnityEngine;
using UnityEngine.InputSystem;

public class SC_PauseManager : MonoBehaviour
{
    public GameObject pause_menu;

    private bool opened = false;

    public void OpenPauseMenu(InputAction.CallbackContext con)
    {
        if (con.performed)
        {
            opened = !opened;
            pause_menu.SetActive(opened);

            if (opened)
            {
                OpenPause();
            }
            else
            {
                ClosePause();
            }
        }
    }
    private void OpenPause()
    {
        Time.timeScale = 0;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    private void ClosePause()
    {
        Time.timeScale = 1;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
}