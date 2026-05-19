using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GrassDensityPainter))]
public class GrassDensityPainterEditor : Editor
{
    bool isPainting = false;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GrassDensityPainter painter = (GrassDensityPainter)target;

        EditorGUILayout.Space();
        isPainting = GUILayout.Toggle(isPainting, isPainting ? "🖌 Painting (click to stop)" : "🖌 Start Painting", "Button", GUILayout.Height(30));

        if (GUILayout.Button("Save Map"))
            painter.SaveDensityMap();

        // Preview the texture
        if (painter.densityMap != null)
        {
            EditorGUILayout.Space();
            GUILayout.Label("Density Map Preview:");
            Rect rect = GUILayoutUtility.GetRect(256, 256);
            EditorGUI.DrawPreviewTexture(rect, painter.densityMap);
        }
    }

    void OnSceneGUI()
    {
        if (!isPainting) return;

        GrassDensityPainter painter = (GrassDensityPainter)target;
        Event e = Event.current;

        // Prevent scene from deselecting object while painting
        HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));

        // Raycast mouse onto the plane
        Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
        Plane plane = new Plane(Vector3.up, painter.transform.position);

        if (plane.Raycast(ray, out float distance))
        {
            Vector3 hitPoint = ray.GetPoint(distance);

            // Convert to UV
            float u = (hitPoint.x - painter.transform.position.x + painter.GetComponent<GrassScatter>().areaWidth / 2) / painter.GetComponent<GrassScatter>().areaWidth;
            float v = (hitPoint.z - painter.transform.position.z + painter.GetComponent<GrassScatter>().areaLength / 2) / painter.GetComponent<GrassScatter>().areaLength;

            // Draw brush preview circle
            Handles.color = painter.eraseMode ? Color.red : Color.white;
            Handles.DrawWireDisc(hitPoint, Vector3.up, painter.brushSize * painter.GetComponent<GrassScatter>().areaWidth / painter.textureResolution);

            if ((e.type == EventType.MouseDown || e.type == EventType.MouseDrag) && e.button == 0)
            {
                painter.Paint(new Vector2(u, v));
                e.Use();
            }
        }

        SceneView.RepaintAll();
    }
}