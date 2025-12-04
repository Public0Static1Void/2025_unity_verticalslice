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

        // Get all subclasses of SC_Mark
        var types = Assembly.GetAssembly(typeof(SC_Mark))
            .GetTypes()
            .Where(t => t.IsSubclassOf(typeof(SC_Mark)) && !t.IsAbstract);

        int index = 0;

        foreach (var type in types)
        {
            if (GUILayout.Button("Add " + type.Name))
            {
                var newComp = manager.gameObject.AddComponent(type);

                // Try to cast to SC_Mark so we can call Init()
                SC_Mark mark = newComp as SC_Mark;
                if (mark != null)
                {
                    // Call your Init with your desired values
                    mark.Init(SC_ElementManager.instance.GetElement((ElementTypes)index));
                }
            }
        }
    }
}