using System;
using UnityEngine;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Unity.Mathematics;


public class SC_GameManager : MonoBehaviour
{
    public TMP_Text txt_announce = null;
    public static SC_GameManager instance {  get; private set; }

    public bool mark_unlocked = false;

    [Header("References")]
    public Image ob_loading_screen = null;
    public Image im_loading_bar = null;
    public Canvas main_canvas;

    private List<ElementTypes> unlocked_elements = new List<ElementTypes>();

    void Awake()
    {
        if (instance != null)
        {
            unlocked_elements = new List<ElementTypes>(instance.unlocked_elements);
            Destroy(instance.gameObject);
            instance = this;
        }
        else
        {
            instance = this;    
        }

        transform.parent = null;
        DontDestroyOnLoad(gameObject);

        Application.targetFrameRate = 60;

        Time.timeScale = 0;


        StartCoroutine(FadeOut());
    }

    private void Start()
    {
        // Get your unlocked marks
        for (int i = 0; i <  unlocked_elements.Count; i++)
        {
            SC_MarksPlacer.Instance.UnlockMark(unlocked_elements[i]);
        }
    }

    private IEnumerator FadeOut()
    {
        SetLoadingAlpha(1);
        ob_loading_screen.gameObject.SetActive(true);

        float alpha = 1;
        while (alpha > 0)
        {
            alpha -= Time.unscaledDeltaTime;
            SetLoadingAlpha(alpha);
            yield return null;
        }
        SetLoadingAlpha(0);
        ob_loading_screen.gameObject.SetActive(false);

        Time.timeScale = 1;

        ob_loading_screen.gameObject.SetActive(false);

        Start();
    }

    /// <summary>
    /// Checks if a LayerMask contains a certain layer
    /// </summary>
    public static bool LayerContainsMask(LayerMask layerMask, string layerName)
    {
        int layer = LayerMask.NameToLayer(layerName);
        return (layerMask.value & (1 << layer)) != 0;
    }
    /// <summary>
    /// Checks if an object contains a certain layer
    /// </summary>
    public static bool ObjectContainsLayer(GameObject obj, LayerMask layer)
    {
        return (layer.value & (1 << obj.layer)) != 0;
    }
    /// <summary>
    /// Returns all the components of type T attached on the parent children
    /// </summary>
    public static T[] GetChildsComponents<T>(Transform parent) where T : Component
    {
        T[] children = new T[parent.childCount];

        for (int i = 0; i < parent.childCount; i++)
        {
            children[i] = parent.GetChild(i).GetComponent<T>();
        }

        return children;
    }

    public static Vector3 LimitVector(Vector3 vector, float max_x, float min_x, float max_y, float min_y, float max_z, float min_z)
    {
        vector.x = Mathf.Clamp(vector.x, min_x, max_x);
        vector.y = Mathf.Clamp(vector.y, min_y, max_y);
        vector.z = Mathf.Clamp(vector.z, min_z, max_z);

        return vector;
    }


    public void ShowText(string text)
    {
        txt_announce.text = text;
        StartCoroutine(ShowTextCoroutine(2));
    }
    private IEnumerator ShowTextCoroutine(float show_speed = 1)
    {
        Color col = txt_announce.color;
        txt_announce.color = new Color(col.r, col.g, col.b, 0);

        float alpha = 0;
        while (alpha < 1)
        {
            alpha += Time.deltaTime * show_speed;

            txt_announce.color = new Color(col.r, col.g, col.b, alpha);
            yield return null;
        }
        yield return new WaitForSeconds(3);
        while (alpha > 0)
        {
            alpha -= Time.deltaTime * show_speed;

            txt_announce.color = new Color(col.r, col.g, col.b, alpha);
            yield return null;
        }

        txt_announce.color = new Color(col.r, col.g, col.b, 0);
    }



    public void LoadSceneAsync(string sceneName)
    {
        unlocked_elements = new List<ElementTypes>(SC_MarksPlacer.Instance.GetUnlockedMarks());
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

        foreach (var txt in GetChildsComponents<TMP_Text>(ob_loading_screen.transform))
        {
            if (txt != null)
            {
                txt.color = new Color(txt.color.r, txt.color.g, txt.color.b, a);
            }
        }
    }
}