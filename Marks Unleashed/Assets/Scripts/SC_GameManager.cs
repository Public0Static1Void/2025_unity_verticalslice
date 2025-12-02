using System;
using UnityEngine;
using System.Collections.Generic;


public class SC_GameManager : MonoBehaviour
{
    public static SC_GameManager instance {  get; private set; }

    void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
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
}