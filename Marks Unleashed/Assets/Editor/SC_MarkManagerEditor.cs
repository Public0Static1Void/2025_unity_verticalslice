#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using System.Reflection;

[CustomEditor(typeof(MarkManager))]
public class MarkManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        GUILayout.Space(10);

        var manager = (MarkManager)target;
        if (manager == null)
            return;

        var baseType = typeof(SC_Mark);
        var assembly = Assembly.GetAssembly(baseType);

        if (assembly == null)
        {
            EditorGUILayout.HelpBox("SC_Mark assembly not found.", MessageType.Error);
            return;
        }

        Type[] types;
        try
        {
            types = assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException e)
        {
            types = e.Types.Where(t => t != null).ToArray();
        }

        foreach (var type in types)
        {
            if (type == null)
                continue;

            if (!type.IsSubclassOf(baseType))
                continue;

            if (type.IsAbstract)
                continue;

            if (!typeof(MonoBehaviour).IsAssignableFrom(type))
                continue;

            // Prevent editor-only scripts
            if (type.Assembly.FullName.Contains("Editor"))
                continue;

            if (GUILayout.Button("Add " + type.Name))
            {
                Undo.AddComponent(manager.gameObject, type);
                EditorUtility.SetDirty(manager.gameObject);
            }
        }
    }
}
#endif