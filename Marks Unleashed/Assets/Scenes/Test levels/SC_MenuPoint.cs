using UnityEngine;

public class SC_MenuPoint : MonoBehaviour
{
    public MenuType menuType;

    public enum MenuType
    {
        MAIN,
        OPTIONS,
        GRAPHICS,
        AUDIO,
        LANGUAGE,
        CREDITS
    }
}
