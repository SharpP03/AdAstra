using UnityEngine;

/// <summary>
/// Shared way of spawning a physical asteroid for all asteroid locations: a sphere collider fitted
/// to the mesh and a mass that grows with the asteroid's volume.
/// </summary>
public static class AsteroidSpawn
{
    /// <summary>
    /// Collision radius of the prefab at scale 1. Lets a location keep asteroids apart before spawning them.
    /// </summary>
    public static float GetRadius(GameObject prefab)
    {
        if (prefab.TryGetComponent(out SphereCollider sc)) return sc.radius;
        if (prefab.TryGetComponent(out MeshFilter mf) && mf.sharedMesh != null)
        {
            Vector3 extents = mf.sharedMesh.bounds.extents;
            return (extents.x + extents.y + extents.z) / 3f;
        }
        return 1f;
    }

    /// <summary>
    /// Spawns an asteroid with a gravity-free rigidbody and a sphere collider.
    /// <paramref name="density"/> is mass per cubic meter of asteroid volume (the player ship weighs 100).
    /// </summary>
    public static Rigidbody Create(GameObject prefab, Vector3 position, Quaternion rotation, float scale, float density, Transform parent)
    {
        GameObject asteroid = Object.Instantiate(prefab, position, rotation, parent);
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
        // Wspólne dla wszystkich lokacji z asteroidami, żeby zderzenia działały wszędzie tak samo.
        #endregion
        float worldRadius = sc.radius * scale;
        rb.mass = density * (4f / 3f) * Mathf.PI * worldRadius * worldRadius * worldRadius;

        return rb;
    }
}
