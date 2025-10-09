using UnityEngine;
[SelectionBase]
public class PathTool : MonoBehaviour
{
    [Header("Path bounds")]
    public Transform startPoint;
    public Transform endPoint;

    [Header("Objects Settings")]
    public GameObject cubePrefab;
    public int cubeCount = 100;
    public float scatterRadius = 10f;  // how far from the line cubes can spawn
    public Vector3 scaleRange = new Vector3(0.5f, 2f, 0.5f); // random size variation

    private void Start()
    {
        if (startPoint == null || endPoint == null || cubePrefab == null)
        {
            Debug.LogWarning("Missing startPoint, endPoint, or cubePrefab!");
            return;
        }

        SpawnScatteredCubes();
    }

    private void SpawnScatteredCubes()
    {
        Vector3 direction = endPoint.position - startPoint.position;

        for (int i = 0; i < cubeCount; i++)
        {
            // pick random point along the line
            float t = Random.Range(0f, 1f);
            Vector3 basePos = Vector3.Lerp(startPoint.position, endPoint.position, t);

            // offset it randomly within a sphere around that line
            Vector3 randomOffset = Random.insideUnitSphere * scatterRadius;
            Vector3 pos = basePos + randomOffset;

            // spawn
            GameObject cube = Instantiate(cubePrefab, pos, Random.rotation, transform);

            // random scale (optional)
            float randomScale = Random.Range(scaleRange.x, scaleRange.y);
            cube.transform.localScale = Vector3.one * randomScale;
        }
    }
}
