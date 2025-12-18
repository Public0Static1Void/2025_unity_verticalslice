#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using System.Reflection;

[CustomEditor(typeof(MarkManager))]
public class MarkManagerEditor : Editor
{
    private static Type[] cachedMarkTypes;

    public override void OnInspectorGUI()
    {
        if (target == null || serializedObject == null)
            return;

        try
        {
            base.OnInspectorGUI();
        }
        catch
        {
            return;
        }

        GUILayout.Space(10);

        var manager = target as MarkManager;
        if (manager == null)
            return;

        if (cachedMarkTypes == null)
        {
            var baseType = typeof(SC_Mark);
            var assembly = Assembly.GetAssembly(baseType);

            try
            {
                cachedMarkTypes = assembly.GetTypes()
                    .Where(t =>
                        t != null &&
                        t.IsSubclassOf(baseType) &&
                        !t.IsAbstract &&
                        typeof(MonoBehaviour).IsAssignableFrom(t) &&
                        !t.Assembly.FullName.Contains("Editor")
                    ).ToArray();
            }
            catch
            {
                cachedMarkTypes = Array.Empty<Type>();
            }
        }

        foreach (var type in cachedMarkTypes)
        {
            if (GUILayout.Button("Add " + type.Name))
            {
                Undo.RecordObject(manager.gameObject, "Add Mark Component");
                manager.gameObject.AddComponent(type);
                EditorUtility.SetDirty(manager.gameObject);
            }
        }
    }
}
#endif
