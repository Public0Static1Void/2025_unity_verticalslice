using TMPro;
using UnityEngine;

public class SC_CodexEntryButton : MonoBehaviour
{
    public TextMeshProUGUI buttonTitleText;
    private SO_CodexEntry assignedEntry;
    private SC_CodexManager codexManager;

    public void Setup(SO_CodexEntry entry, SC_CodexManager manager)
    {
        assignedEntry = entry;
        codexManager = manager;

        buttonTitleText.text = entry.title;
    }

    public void OnButtonClick()
    {
        codexManager.DisplayCodexEntry(assignedEntry);
    }
}
