using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteInEditMode]
public class GrassDensityPainter : MonoBehaviour
{
    [Header("Texture Settings")]
    public int textureResolution = 512;
    public Texture2D densityMap;

    [Header("Brush Settings")]
    public float brushSize = 20f;
    [Range(0f, 1f)] public float brushStrength = 1f;
    public bool eraseMode = false;

#if UNITY_EDITOR
    [ContextMenu("Create New Density Map")]
    public void CreateDensityMap()
    {
        string basePath = "Assets/GrassMaps/GrassDensityMap";
        string path = basePath + ".png";
        int index = 1;
        while (System.IO.File.Exists(path))
        {
            path = basePath + "_" + index + ".png";
            index++;
        }

        Texture2D tex = new Texture2D(textureResolution, textureResolution, TextureFormat.RGB24, false);
        Color[] pixels = new Color[textureResolution * textureResolution];
        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = Color.black;
        tex.SetPixels(pixels);
        tex.Apply();

        System.IO.Directory.CreateDirectory("Assets/GrassMaps");
        System.IO.File.WriteAllBytes(path, tex.EncodeToPNG());
        AssetDatabase.Refresh();

        TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(path);
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.isReadable = true;
        importer.mipmapEnabled = false;
        importer.SaveAndReimport();

        densityMap = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        Debug.Log("Density map created at " + path);
    }

    [ContextMenu("Save Density Map")]
    public void SaveDensityMap()
    {
        if (densityMap == null) return;
        string path = AssetDatabase.GetAssetPath(densityMap);
        System.IO.File.WriteAllBytes(path, densityMap.EncodeToPNG());
        AssetDatabase.Refresh();
        Debug.Log("Saved!");
    }
#endif

    // Paint has no editor dependencies — stays outside the guard
    public void Paint(Vector2 uv)
    {
        if (densityMap == null) return;

        int centerX = (int)(uv.x * textureResolution);
        int centerY = (int)(uv.y * textureResolution);
        int radius = (int)(brushSize);
        Color paintColor = eraseMode ? Color.black : Color.white;

        for (int x = centerX - radius; x < centerX + radius; x++)
        {
            for (int y = centerY - radius; y < centerY + radius; y++)
            {
                if (x < 0 || x >= textureResolution || y < 0 || y >= textureResolution)
                    continue;

                float dist = Vector2.Distance(new Vector2(x, y), new Vector2(centerX, centerY));
                if (dist > radius) continue;

                float falloff = 1f - (dist / radius);
                Color current = densityMap.GetPixel(x, y);
                densityMap.SetPixel(x, y, Color.Lerp(current, paintColor, falloff * brushStrength));
            }
        }
        densityMap.Apply();
    }
}