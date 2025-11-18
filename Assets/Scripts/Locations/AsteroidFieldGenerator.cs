using UnityEngine;

public class AsteroidFieldGenerator : MonoBehaviour
{
    public GameObject[] asteroidPrefabs;
    public int count = 200;
    public float radius = 300f;
    public float forceStrength = 2f;
    public float scaleMultiplier = 1.5f;
    public float rotationMultiplier = 1f;


    void Start()
    {
        for (int i = 0; i < count; i++)
        {
            GameObject prefab = asteroidPrefabs[Random.Range(0, asteroidPrefabs.Length)];
            Vector3 pos = transform.position + Random.insideUnitSphere * radius;
            Quaternion rot = Random.rotation;
            float scale = Random.Range(0.5f * scaleMultiplier, 2.5f * scaleMultiplier);

            GameObject asteroid = Instantiate(prefab, pos, rot, transform);
            asteroid.transform.localScale = Vector3.one * scale;

            Rigidbody rb = asteroid.GetComponent<Rigidbody>();
            if (rb == null)
            {
                rb = asteroid.AddComponent<Rigidbody>();
                rb.useGravity = false;
            }

            SphereCollider sc = asteroid.GetComponent<SphereCollider>();
            if (sc == null)
            {
                sc = asteroid.AddComponent<SphereCollider>();
                sc.radius = 2f * scale; 
            }

            Vector3 randomDirection = Random.onUnitSphere.normalized;
            rb.AddForce(randomDirection * forceStrength, ForceMode.Impulse);
            rb.angularVelocity = Random.onUnitSphere * Random.Range(0.1f * rotationMultiplier, 0.5f * rotationMultiplier);
        }
    }
}
