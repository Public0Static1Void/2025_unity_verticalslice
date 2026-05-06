using System;
using UnityEngine;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Unity.Mathematics;
using UnityEngine.Video;


public class SC_GameManager : MonoBehaviour
{
    public TMP_Text txt_announce = null;
    public static SC_GameManager instance {  get; private set; }

    public bool mark_unlocked = false;

    [Header("Stats")]
    public bool destroy_on_load = false;

    [Header("References")]
    public Image ob_loading_screen = null;
    public Image im_loading_bar = null;
    public Canvas main_canvas;
    public GameObject im_stone_tut;
    public GameObject im_ice_tut;
    public GameObject im_air_tut;
    public VideoPlayer video_stone;
    public VideoPlayer video_air;
    public VideoPlayer video_ice;

    private List<ElementTypes> unlocked_elements = new List<ElementTypes>();

    public bool first_play = true;

    [Space(15)]
    [Header("Translation")]
    public List<TMP_Text> texts_to_translate = new List<TMP_Text>();
    private List<string> texts_keys = new List<string>();
    private SC_IdiomManager _idiomManager;

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
        if (!destroy_on_load)
            DontDestroyOnLoad(gameObject);

        Application.targetFrameRate = 60;

        Time.timeScale = 0;

        if (!destroy_on_load)
            StartCoroutine(FadeOut());

        foreach (TMP_Text txt in texts_to_translate)
        {
            texts_keys.Add(txt.text);
        }
    }

    private void Start()
    {
        _idiomManager = SC_IdiomManager.instance;
        LoadTexts();
        // Get your unlocked marks
        for (int i = 0; i < unlocked_elements.Count; i++)
        {
            SC_MarksPlacer.Instance.UnlockMark(unlocked_elements[i]);
        }
    }
    public void LoadTexts()
    {
        // Translate the scene texts
        for (int i = 0; i <  texts_keys.Count; i++)
        {
            if (i < texts_keys.Count)
                texts_to_translate[i].text = _idiomManager.GetText(texts_keys[i]);
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

    public static bool AreColorsSimilar(Color a, Color b, float threshold)
    {
        return Mathf.Abs(a.r - b.r) < threshold &&
               Mathf.Abs(a.g - b.g) < threshold &&
               Mathf.Abs(a.b - b.b) < threshold &&
               Mathf.Abs(a.a - b.a) < threshold;
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


    public void ShowTutorial(string tutorial, float duration)
    {
        if (tutorial == "") return;

        GameObject im_tut = null;
        VideoClip clip = null;
        switch (tutorial)
        {
            case "Tierra":
                im_tut = im_stone_tut;
                im_tut.SetActive(true);
                video_stone.Play();
                clip = video_stone.clip;
                break;
            case "Aire":
                im_tut = im_air_tut;
                im_tut.SetActive(true);
                video_air.Play();
                clip = video_air.clip;
                break;
            case "Hielo":
                im_tut = im_ice_tut;
                im_tut.SetActive(true);
                video_ice.Play();
                clip = video_ice.clip;
                break;
        }

        Destroy(im_tut, (float)clip.length);
    }

    public void IncreaseLights(GameObject ob)
    {
        Light[] lights = GetChildsComponents<Light>(ob.transform);
        IncreaseLights(lights, 1);
    }
    public void IncreaseLights(Light[] lights, float duration)
    {
        StartCoroutine(IncreaseLightsRoutine(lights, duration));
    }
    private IEnumerator IncreaseLightsRoutine(Light[] lights, float duration)
    {
        List<float> intensities = new List<float>();
        foreach (Light l in lights)
        {
            if (l == null) continue;
            intensities.Add(l.intensity);
        }

        float timer = 0;
        while (timer < duration)
        {
            float t = timer / duration;
            for (int i = 0; i < intensities.Count && i < lights.Length; i++)
            {
                lights[i].intensity = intensities[i] * t;
            }

            timer += Time.deltaTime;
            yield return null;
        }
    }

    private IEnumerator ShowImageRoutine(Image im, float duration)
    {
        float timer = 0;

        Color col = im.color;
        col.a = 0;
        im.color = col;
        while (timer < duration)
        {
            timer += Time.deltaTime;

            col.a = 255 * timer;
            im.color = col;

            yield return null;
        }

        col.a = 255;
        im.color = col;

        yield return new WaitForSeconds(30);
        HideImageRoutine(im, duration * 2);
    }
    private IEnumerator HideImageRoutine(Image im, float duration)
    {
        float timer = 0;

        Color col = im.color;
        col.a = 255;
        im.color = col;
        while (timer < duration)
        {
            timer += Time.deltaTime;

            col.a = 255 * (1 - (timer / duration));
            im.color = col;

            yield return null;
        }

        col.a = 0;
        im.color = col;
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