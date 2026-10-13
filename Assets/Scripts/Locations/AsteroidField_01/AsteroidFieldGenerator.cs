using UnityEngine;

public class AsteroidFieldGenerator : MonoBehaviour
{
    public GameObject[] asteroidPrefabs;
    public int count = 200;
    public float radius = 300f;
    public float forceStrength = 2f;
    public float scaleMultiplier = 1.5f;
    public float rotationMultiplier = 1f;
    [Tooltip("Mass per cubic meter of asteroid volume. The player ship weighs 100.")]
    public float density = 0.5f;


    void Start()
    {
        for (int i = 0; i < count; i++)
        {
            GameObject prefab = asteroidPrefabs[Random.Range(0, asteroidPrefabs.Length)];
            Vector3 pos = transform.position + Random.insideUnitSphere * radius;
            Quaternion rot = Random.rotation;
            float scale = Random.Range(0.5f * scaleMultiplier, 2.5f * scaleMultiplier);

            Rigidbody rb = AsteroidSpawn.Create(prefab, pos, rot, scale, density, transform);

            // VelocityChange keeps the drift speed independent of the asteroid mass.
            Vector3 randomDirection = Random.onUnitSphere.normalized;
            rb.AddForce(randomDirection * forceStrength, ForceMode.VelocityChange);
            rb.angularVelocity = Random.onUnitSphere * Random.Range(0.1f * rotationMultiplier, 0.5f * rotationMultiplier);
        }
    }
}
