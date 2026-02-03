using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SC_MenuManager : MonoBehaviour
{
    public AudioSource audioSource;

    public UnityEngine.UI.Image ob_loading_screen;
    public UnityEngine.UI.Image im_loading_bar;

    [SerializeField] private GameObject _optionsMenu;
    [SerializeField] private GameObject _creditsMenu;

    private void Start()
    {
        CloseAllMenus();
    }
    public void CloseAllMenus() //Cierra cualquier menu que se haya quedado abierto
    {
        _optionsMenu.SetActive(false);
        _creditsMenu.SetActive(false);
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

        foreach (var img in SC_GameManager.GetChildsComponents<UnityEngine.UI.Image>(ob_loading_screen.transform))
        {
            if (img != null)
                img.color = new Color(img.color.r, img.color.g, img.color.b, a);
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