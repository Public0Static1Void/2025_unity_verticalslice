using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(GrassDensityPainter))]
public class GrassScatter : MonoBehaviour
{
    [Header("Grass Settings")]
    public GameObject grassPrefab;
    public int grassCount = 500;
    public float areaWidth = 10f;
    public float areaLength = 10f;
    public Vector3 max_scale, min_scale;
    public Vector3 rotation;

    [Header("Density Map")]
    public Texture2D densityMap;
    [Range(0f, 1f)] public float densityThreshold = 0.5f;

    [ContextMenu("Scatter Grass")]
    public void ScatterGrass()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
            DestroyImmediate(transform.GetChild(i).gameObject);

        int placed = 0;
        int attempts = 0;

        Renderer planeRenderer = GetComponent<Renderer>();
        Bounds bounds = planeRenderer.bounds;

        while (placed < grassCount && attempts < grassCount * 10)
        {
            attempts++;

            // Sample directly from world-space bounds min/max
            Vector3 randomPos = new Vector3(
                Random.Range(bounds.min.x, bounds.max.x),
                bounds.min.y,
                Random.Range(bounds.min.z, bounds.max.z)
            );

            if (densityMap != null)
            {
                // UV mapping now based on bounds too (consistent)
                float u = (randomPos.x - bounds.min.x) / bounds.size.x;
                float v = (randomPos.z - bounds.min.z) / bounds.size.z;
                float pixelValue = densityMap.GetPixelBilinear(u, v).grayscale;
                if (pixelValue < densityThreshold)
                    continue;
            }

            Quaternion randomRot = Quaternion.Euler(rotation.x, rotation.y, rotation.z);
            GameObject inst = Instantiate(grassPrefab, Vector3.zero, randomRot, transform);

            // Apply scale first so bounds are correct
            float scale_x = Random.Range(min_scale.x, max_scale.x);
            Vector3 scale = new Vector3(
                scale_x,
                Random.Range(min_scale.y, max_scale.y),
                scale_x
            );
            inst.transform.localScale = scale;

            // Get the rendered bounds to find the bottom of the mesh
            Renderer rend = inst.GetComponentInChildren<Renderer>();
            float bottomOffset = 0f;
            if (rend != null)
            {
                // How far is the pivot from the bottom of the mesh?
                bottomOffset = inst.transform.position.y - rend.bounds.min.y;
            }

            // Place at parent height + offset so bottom of mesh sits exactly on the plane
            Vector3 finalPos = transform.position + new Vector3(randomPos.x, bottomOffset, randomPos.z);
            inst.transform.position = finalPos;

            placed++;
        }
    }

    [ContextMenu("Clear Grass")]
    public void ClearGrass()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
            DestroyImmediate(transform.GetChild(i).gameObject);
    }
}