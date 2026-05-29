using UnityEngine;
using UnityEngine.UI;

public class SC_HighlightUsedLanguageButton : MonoBehaviour
{
    public Button englishButton;
    public Button spanishButton;
    public Button catalanButton;

    public void Start()
    {
        RefreshHighlightedButton();
    }

    public void RefreshHighlightedButton()
    {
        englishButton.interactable = true;
        spanishButton.interactable = true;
        catalanButton.interactable = true;

        switch (SC_LanguageManager.instance._currentLanguage) 
        {
            case SC_LanguageManager.Language.ENGLISH:
                englishButton.interactable = false;
                break;

            case SC_LanguageManager.Language.SPANISH:
                spanishButton.interactable = false;
                break;

            case SC_LanguageManager.Language.CATALAN:
                catalanButton.interactable = false;
                break;

            default:
                englishButton.interactable = false;
                break;
        }
    }
}
