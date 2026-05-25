using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SC_PauseManager : MonoBehaviour
{
    Resolution[] screenResolutions;

    public GameObject pause_menu;
    public GameObject options_menu;

    public TMP_Dropdown resolutionDropdown;
    public Toggle fullscreenToggle;

    public AudioSource audioSource;
    public AudioMixer audioMixer;
    public SC_PlayerAnimations playerAnimations;
    public SC_CameraRotation cam;
    public SC_CodexManager codexManager;
    public Animator arm_animator;

    [SerializeField] private Volume blurVolume;
    private DepthOfField dof;

    [Header("Audio Sliders")]
    [SerializeField] private Slider _masterSlider;
    [SerializeField] private Slider _musicSlider;
    [SerializeField] private Slider _sfxSlider;
    
    public bool isPauseMenuOpen = false;

    private void Start()
    {
        //Blur empieza false
        if(blurVolume.profile.TryGet(out dof))
        {
            dof.active = false;
        }

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

    public void SetBlur(bool state)
    {
        if (dof != null)
        {
            dof.active = state;
        }
    }

    public void TogglePause()
    {
        isPauseMenuOpen = !isPauseMenuOpen;
        pause_menu.SetActive(isPauseMenuOpen);

        if (isPauseMenuOpen)
        {
            arm_animator.SetBool("pause", true);
            cam.canMoveCamera = false;
            codexManager.CloseCodex();
            SetBlur(true);
            OpenPause();
        }
        else
        {
            arm_animator.SetBool("pause", false);
            cam.canMoveCamera = true;
            SetBlur(false);
            ClosePause();
        }
    }
    public void OpenPauseMenuOnKeyPressed(InputAction.CallbackContext con)
    {
        if (!con.performed) return;

        TogglePause();
    }

    private void OpenPause()
    {
        codexManager.CloseCodex();
        arm_animator.Play("anim_toon_arm_pause");
        //Time.timeScale = 0;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }
    private void ClosePause()
    {
        Time.timeScale = 1;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
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

    // --- AUDIO ---
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
    public void PlaySound(AudioClip clip)
    {
        audioSource.clip = clip;
        audioSource.Play();
    }

    public void BackToMenu()
    {
        SceneManager.LoadScene(0);
    }
}