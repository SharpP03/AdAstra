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
                // Radius in local space: the transform scale is applied on top of it.
                Bounds meshBounds = asteroid.GetComponent<MeshFilter>().sharedMesh.bounds;
                sc.center = meshBounds.center;
                sc.radius = (meshBounds.extents.x + meshBounds.extents.y + meshBounds.extents.z) / 3f;
            }

            #region EDUCATION NOTE asteroid mass
            // DO PRACY INŻ. - masa proporcjonalna do objętości (r^3), a nie stała.
            // Drobne odłamki są lekkie i odlatują po trafieniu, a duże głazy są cięższe od statku,
            // więc wyhamowują go i odbijają - zderzenie jest odczuwalne, nie tylko widoczne na pasku kadłuba.
            #endregion
            float worldRadius = sc.radius * scale;
            rb.mass = density * (4f / 3f) * Mathf.PI * worldRadius * worldRadius * worldRadius;

            // VelocityChange keeps the drift speed independent of the asteroid mass.
            Vector3 randomDirection = Random.onUnitSphere.normalized;
            rb.AddForce(randomDirection * forceStrength, ForceMode.VelocityChange);
            rb.angularVelocity = Random.onUnitSphere * Random.Range(0.1f * rotationMultiplier, 0.5f * rotationMultiplier);
        }
    }
}
