using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SC_CodexManager : MonoBehaviour
{
    [SerializeField] private Transform buttonContainer;
    [SerializeField] private GameObject entryButtonPrefab;
    [SerializeField] private TextMeshProUGUI entryTitleText;
    [SerializeField] private TextMeshProUGUI entryDescriptionText;
    [SerializeField] private Image entryImage;

    [Header("Unlocked Entries")]
    [SerializeField] private List<SO_CodexEntry> unlockedEntries = new List<SO_CodexEntry>();


    private void Start()
    {
        GenerateEntryButtons();
    }

    private void GenerateEntryButtons()
    {
        foreach (SO_CodexEntry entry in unlockedEntries) 
        {
            GameObject buttonObject = Instantiate(entryButtonPrefab, buttonContainer);
            SC_CodexEntryButton entryButton = buttonObject.GetComponent<SC_CodexEntryButton>();
            entryButton.Setup(entry, this);
        }
    }

    public void DisplayCodexEntry(SO_CodexEntry entry)
    {
        if (entry != null)
        {
            entryTitleText.text = entry.title;
            entryDescriptionText.text = entry.desc;
            entryImage.sprite = entry.image;
        }
    }
}
