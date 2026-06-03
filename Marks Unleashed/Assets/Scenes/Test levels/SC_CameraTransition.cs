using UnityEngine;

//Para añadir otro menu:
/*
    1 Añadir nombre del menu en el enum de SC_MenuPoint
    2 Crear una función en este script para mover la camara al nuevo menú con esta estructura:
        public void GoToMain()
        {
            MoveToMenu(MenuType.MAIN);
        }
 */
public class SC_CameraTransition : MonoBehaviour
{
    [Header("Transition Speed")]
    public float moveSpeed = 300f;
    public float rotSpeed = 120f;

    private Transform currentTarget;
    private bool isMoving = false;
    private SC_MenuPoint[] menuPoints;

    private void Start()
    {
        menuPoints = FindObjectsOfType<SC_MenuPoint>();
    }

    private void Update()
    {
        if (!isMoving || currentTarget == null) return;
        
        transform.position = Vector3.Lerp(transform.position, currentTarget.position, Time.deltaTime * moveSpeed);
        transform.rotation = Quaternion.Lerp(transform.rotation, currentTarget.rotation, Time.deltaTime * moveSpeed);
    }

    public void MoveToMenu(SC_MenuPoint.MenuType menu)
    {
        foreach (SC_MenuPoint point in menuPoints) 
        {
            if (point.menuType == menu) 
            {
                currentTarget = point.transform;
                isMoving = true;

                return;
            }
        }
    }

    #region Funciones para mover Cam
    public void GoToMain()
    {
        MoveToMenu(SC_MenuPoint.MenuType.MAIN);
    }
    public void GoToOptions()
    {
        MoveToMenu(SC_MenuPoint.MenuType.OPTIONS);
    }
    public void GoToGraphics()
    {
        MoveToMenu(SC_MenuPoint.MenuType.GRAPHICS);
    }
    public void GoToAudio()
    {
        MoveToMenu(SC_MenuPoint.MenuType.AUDIO);
    }
    public void GoToLanguage()
    {
        MoveToMenu(SC_MenuPoint.MenuType.LANGUAGE);
    }
    public void GoToCredits()
    {
        MoveToMenu(SC_MenuPoint.MenuType.CREDITS);
    }
    #endregion
}
