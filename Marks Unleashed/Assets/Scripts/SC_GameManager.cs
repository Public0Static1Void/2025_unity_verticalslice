using UnityEngine;

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

    public static bool LayerContainsMask(LayerMask layerMask, string layerName)
    {
        int layer = LayerMask.NameToLayer(layerName);
        return (layerMask.value & (1 << layer)) != 0;
    }
    public static bool ObjectContainsLayer(GameObject obj, LayerMask layer)
    {
        return (layer.value & (1 << obj.layer)) != 0;
    }
}