using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

public static class MissingScriptCleanup
{
    [MenuItem("Tools/Cleanup Missing Scripts")]
    static void Cleanup()
    {
        foreach (var go in Object.FindObjectsOfType<GameObject>())
        {
            GameObjectUtility.RemoveMonoBehavioursWithMissingScript(go);
        }

        Debug.Log("Missing scripts removed.");
    }
}
#endif