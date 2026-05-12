using System.Collections.Generic;
using UnityEngine;

public class SC_LanguageManager : MonoBehaviour
{
    public static SC_LanguageManager instance { get; private set; }
    public enum Language { SPANISH, ENGLISH, CATALAN }

    public Language _currentLanguage = Language.ENGLISH;

    Dictionary<string, string[]> localizedTexts = new Dictionary<string, string[]>();

    void Awake()
    {
        if (instance == null)
            instance = this;
        else
        {
            Destroy(gameObject);
        }

        DontDestroyOnLoad(gameObject);

        LoadCSV("idioms");
    }

    void LoadCSV(string fileName)
    {
        TextAsset file = Resources.Load<TextAsset>(fileName);

        if (file == null)
        {
            Debug.LogError("CSV file not found!");
            return;
        }

        string[] lines = file.text.Split('\n');

        for (int i = 1; i < lines.Length; i++) // skip header
        {
            if (string.IsNullOrWhiteSpace(lines[i]))
                continue;

            string[] columns = lines[i].Split(',');

            string key = columns[0].Trim();
            string spanish = columns[1].Trim();
            string english = columns[2].Trim();
            string catalan = columns[3].Trim();

            localizedTexts[key] = new string[] { spanish, english, catalan };
        }
    }

    public string GetText(string key)
    {
        if (!localizedTexts.ContainsKey(key))
        {
            Debug.LogWarning($"No key found: {key}");
            return key;
        }

        string[] values = localizedTexts[key];

        int index = (int)_currentLanguage;
        if (index < 0 || index >= values.Length)
        {
            Debug.LogWarning($"Index out of bounds: {key}");
            return key;
        }

        return values[index];
    }

    public void ChangeLanguage(string language)
    {
        switch (language)
        {
            default:
                _currentLanguage = Language.ENGLISH;
                break;
            case "spain":
                _currentLanguage = Language.SPANISH;
                break;
            case "uk":
                _currentLanguage = Language.ENGLISH;
                break;
            case "cat":
                _currentLanguage = Language.CATALAN;
                break;
        }
        
        SC_GameManager.instance.LoadTexts();
    }
}
