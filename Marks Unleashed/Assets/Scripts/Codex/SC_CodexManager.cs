using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SC_CodexManager : MonoBehaviour
{
    [SerializeField] private bool isCodexOpen = false;

    [Header("References")]
    [SerializeField] private Transform buttonContainer;
    [SerializeField] private GameObject entryButtonPrefab;
    [SerializeField] private TextMeshProUGUI entryTitleText;
    [SerializeField] private TextMeshProUGUI entryDescriptionText;
    [SerializeField] private Image entryImage;
    [SerializeField] private GameObject codexPanel;
    [SerializeField] private SC_PauseManager pauseManager;

    [Header("Unlocked Entries")]
    [SerializeField] private List<SO_CodexEntry> unlockedEntries = new List<SO_CodexEntry>();


    private void Start()
    {
        GenerateEntryButtons();
        codexPanel.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (!pauseManager.isPauseMenuOpen)
            {
                ToggleCodex();
            }
        }
    }

    public void UnlockEntry(SO_CodexEntry entry)
    {
        if (unlockedEntries.Contains(entry))
        {
            return;
        }

        unlockedEntries.Add(entry);

        CreateEntryButton(entry);
    }

    public void ToggleCodex()
    {
        if (isCodexOpen)
        {
            CloseCodex();
        }
        else
        {
            OpenCodex();
        }
    }

    public void OpenCodex()
    {
        //Time.timeScale = 0;
        isCodexOpen = true;
        codexPanel.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void CloseCodex()
    {
        //Time.timeScale = 1;
        isCodexOpen = false;
        codexPanel.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void CreateEntryButton(SO_CodexEntry entry)
    {
        GameObject buttonObject = Instantiate(entryButtonPrefab, buttonContainer);

        SC_CodexEntryButton buttonUI = buttonObject.GetComponent<SC_CodexEntryButton>();

        buttonUI.Setup(entry, this);
    }

    private void GenerateEntryButtons()
    {
        foreach (SO_CodexEntry entry in unlockedEntries)
        {
            CreateEntryButton(entry);
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
