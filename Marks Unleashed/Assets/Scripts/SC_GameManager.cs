using System;
using UnityEngine;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro;
using System.Collections;


public class SC_GameManager : MonoBehaviour
{
    public TMP_Text txt_announce = null;
    public static SC_GameManager instance {  get; private set; }

    public bool mark_unlocked = false;

    void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);

        Application.targetFrameRate = 60;
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
}