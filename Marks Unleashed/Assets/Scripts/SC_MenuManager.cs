using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SC_MenuManager : MonoBehaviour
{
    Resolution[] screenResolutions;

    public TMP_Dropdown resolutionDropdown;
    public Toggle fullscreenToggle;

    public AudioSource audioSource;
    public AudioMixer audioMixer;

    public UnityEngine.UI.Image ob_loading_screen;
    public UnityEngine.UI.Image im_loading_bar;

    [Header("Menu Panels")]
    [SerializeField] private GameObject _optionsMenu;
    [SerializeField] private GameObject _creditsMenu;

    [Header("Audio Sliders")]
    [SerializeField] private Slider _masterSlider;
    [SerializeField] private Slider _musicSlider;
    [SerializeField] private Slider _sfxSlider;

    private void Start()
    {
        //CloseAllMenus();

        screenResolutions = Screen.resolutions;
        resolutionDropdown.ClearOptions();

        List<string> options = new List<string>();
        int currentResolutionIndex = 0;

        for (int i = 0; i < screenResolutions.Length; i++)
        {
            string option = screenResolutions[i].width + " x " + screenResolutions[i].height;
            options.Add(option);

            if (screenResolutions[i].width == Screen.currentResolution.width &&
                screenResolutions[i].height == Screen.currentResolution.height)
            {
                currentResolutionIndex = i;
            }
        }

        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = currentResolutionIndex;
        resolutionDropdown.RefreshShownValue();

        fullscreenToggle.isOn = Screen.fullScreen;

        _masterSlider.onValueChanged.AddListener(SetMasterVolume);
        _musicSlider.onValueChanged.AddListener(SetMusicVolume);
        _sfxSlider.onValueChanged.AddListener(SetSFXVolume);

        //Carga volumen de PlayerPrefs
        _masterSlider.value = PlayerPrefs.GetFloat("MasterVolume", 0f);
        _musicSlider.value = PlayerPrefs.GetFloat("MusicVolume", 0f);
        _sfxSlider.value = PlayerPrefs.GetFloat("SFXVolume", 0f);

        SetMasterVolume(_masterSlider.value);
        SetMusicVolume(_musicSlider.value);
        SetSFXVolume(_sfxSlider.value);

    }

    public void SetResolution(int resolutionIndex)
    {
        Resolution resolution = screenResolutions[resolutionIndex];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
    }

    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;

        // Re-apply resolution so it switches cleanly
        Resolution resolution = screenResolutions[resolutionDropdown.value];
        Screen.SetResolution(resolution.width, resolution.height, isFullscreen);
    }

    public void SetMasterVolume(float _value)
    {
        audioMixer.SetFloat("MasterVolume", _value);
        PlayerPrefs.SetFloat("MasterVolume", _value);
    }
    public void SetMusicVolume(float _value)
    {
        audioMixer.SetFloat("MusicVolume", _value);
        PlayerPrefs.SetFloat("MusicVolume", _value);
    }
    public void SetSFXVolume(float _value)
    {
        audioMixer.SetFloat("SFXVolume", _value);
        PlayerPrefs.SetFloat("SFXVolume", _value);
    }

    public void CloseAllMenus() //Cierra cualquier menu que se haya quedado abierto
    {
        _optionsMenu.SetActive(false);
        if(_creditsMenu != null)
        {
            _creditsMenu.SetActive(false);
        }
    }

    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    public void PlaySound(AudioClip clip)
    {
        audioSource.clip = clip;
        audioSource.Play();
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void LoadSceneAsync(string sceneName)
    {
        StartCoroutine(LoadSceneAsyncCoroutine(sceneName));
    }
    private IEnumerator LoadSceneAsyncCoroutine(string sceneName)
    {
        Time.timeScale = 0;

        ob_loading_screen.gameObject.SetActive(true);

        float alpha = 0;

        // Fade in
        while (alpha < 1)
        {
            alpha += Time.unscaledDeltaTime;
            SetLoadingAlpha(alpha);

            im_loading_bar.fillAmount = alpha;

            yield return null;
        }
        yield return new WaitForSecondsRealtime(UnityEngine.Random.Range(0.75f, 1.5f));

        Time.timeScale = 1;
        SceneManager.LoadScene(sceneName);
    }

    void SetLoadingAlpha(float a)
    {
        Color col = ob_loading_screen.color;
        ob_loading_screen.color = new Color(col.r, col.g, col.b, a);


        Image[] images = ob_loading_screen.GetComponentsInChildren<Image>(true);

        foreach (var img in images)
        {
            if (img != null)
            {
                Color c = img.color;
                img.color = new Color(c.r, c.g, c.b, a);
            }
        }

        foreach (var txt in SC_GameManager.GetChildsComponents<TMP_Text>(ob_loading_screen.transform))
        {
            if (txt != null)
            {
                txt.color = new Color(txt.color.r, txt.color.g, txt.color.b, a);
            }
        }
    }
}