using UnityEngine;
using static SC_MenuPoint;

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
    public float moveSpeed = 3f;

    private Transform currentTarget; //Punto al que se dirige la camara
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

        //Sin este snap la camara se "acerca todo el rato"
        float distance = Vector3.Distance(transform.position, currentTarget.position);
        if (distance < 0.0001) 
        {
            transform.position = currentTarget.position;
            transform.rotation = currentTarget.rotation;
            isMoving = false;
        }
    }

    public void MoveToMenu(MenuType menu)
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
        MoveToMenu(MenuType.MAIN);
    }

    public void GoToOptions()
    {
        MoveToMenu(MenuType.OPTIONS);
    }

    public void GoToCredits()
    {
        MoveToMenu(MenuType.CREDITS);
    }
    #endregion
}
