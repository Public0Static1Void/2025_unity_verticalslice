using UnityEngine;

[CreateAssetMenu(fileName = "New Codex Entry", menuName = "Codex/Codex Entry")]
public class SO_CodexEntry : ScriptableObject
{
    public string entryID; //Indentificador único de entrada | Formato: todo minúsculas, espacios con _ ej: runa_tierra

    public string title; //Texto que se mostrará en la entrada y título de la entrada del códice

    public Sprite image; //Imagen que se verá en la entrada

    [TextArea(5, 15)]
    public string desc; //Texto que se muestra en la parte derecha del codex
}
