using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Asteroid stream across the route: a flat box of asteroids in which random asteroids, each on its own
/// timer, glow for a moment and then get kicked, mostly back and forth across the route. Asteroids that leave
/// the box turn back softly and slow down between impulses, so the stream stays in place without a fixed pattern.
/// A pure hazard: crossing it costs hull, flying around it costs time. The Stasis Core in the middle is optional.
/// </summary>
public class AsteroidStream : MonoBehaviour
{
    [SerializeField] private AsteroidStreamConfigSO config;

    private class Asteroid
    {
        public Rigidbody Body;
        public Renderer Renderer;
        public Material BaseMaterial;
        public float NextImpulseTime;
        public bool Telegraphing;
    }

    private readonly List<Asteroid> asteroids = new List<Asteroid>();
    private readonly Dictionary<Material, Material> glowMaterials = new Dictionary<Material, Material>();
    private readonly List<Vector4> placed = new List<Vector4>(); // xyz = local position, w = radius

    private void Start()
    {
        if (config == null || config.asteroidPrefabs == null || config.asteroidPrefabs.Length == 0)
        {
            Debug.LogError("AsteroidStream: missing config or asteroid prefabs.", this);
            enabled = false;
            return;
        }

        // A fixed seed must not change the random sequence of other systems.
        Random.State previousState = Random.state;
        if (config.seed != 0) Random.InitState(config.seed);

        for (int i = 0; i < config.count; i++) TrySpawn();

        if (config.seed != 0) Random.state = previousState;
    }

    private void OnDestroy()
    {
        foreach (Material glow in glowMaterials.Values) Destroy(glow);
    }

    private void FixedUpdate()
    {
        Vector3 halfSize = config.size * 0.5f;

        foreach (Asteroid a in asteroids)
        {
            if (a.Body == null) continue;

            UpdateImpulse(a);

            #region EDUCATION NOTE soft boundary
            // DO PRACY INŻ. - miękka granica zamiast odbicia od niewidzialnej ściany.
            // Asteroida, która wyleci poza prostopadłościan, dostaje stałe przyspieszenie z powrotem (osobno w każdej osi).
            // Prędkość nie zmienia znaku w jednej klatce, tylko maleje i odwraca się po łuku - ruch wygląda jak rój,
            // a asteroida wypchnięta przez statek wraca sama. Wyjście poza obszar jest ograniczone przez v² / (2·a).
            #endregion
            Vector3 local = transform.InverseTransformPoint(a.Body.position);
            Vector3 turnBack = new Vector3(
                ReturnAxis(local.x, halfSize.x),
                ReturnAxis(local.y, halfSize.y),
                ReturnAxis(local.z, halfSize.z)) * config.returnAcceleration;
            if (turnBack != Vector3.zero)
                a.Body.AddForce(transform.TransformDirection(turnBack), ForceMode.Acceleration);

            if (a.Body.linearVelocity.sqrMagnitude > config.maxSpeed * config.maxSpeed)
                a.Body.linearVelocity = a.Body.linearVelocity.normalized * config.maxSpeed;
        }
    }

    private static float ReturnAxis(float position, float halfExtent)
    {
        if (position > halfExtent) return -1f;
        if (position < -halfExtent) return 1f;
        return 0f;
    }

    private void UpdateImpulse(Asteroid a)
    {
        float time = Time.time;

        // The glow announces the impulse, so a hit is a readable risk and not a lottery.
        if (!a.Telegraphing && time >= a.NextImpulseTime - config.telegraphTime)
        {
            a.Telegraphing = true;
            if (a.Renderer != null) a.Renderer.sharedMaterial = GetGlowMaterial(a.BaseMaterial);
        }

        if (time < a.NextImpulseTime) return;

        a.Telegraphing = false;
        if (a.Renderer != null) a.Renderer.sharedMaterial = a.BaseMaterial;

        // VelocityChange keeps the kick independent of the asteroid mass.
        a.Body.AddForce(transform.TransformDirection(ImpulseDirection()) * Random.Range(config.impulseSpeed.x, config.impulseSpeed.y), ForceMode.VelocityChange);
        a.NextImpulseTime = time + Random.Range(config.impulseInterval.x, config.impulseInterval.y);
    }

    /// <summary>Local direction of an impulse: random, pulled toward ±X by the axis bias.</summary>
    private Vector3 ImpulseDirection()
    {
        Vector3 axis = Random.value < 0.5f ? Vector3.right : Vector3.left;
        Vector3 direction = Vector3.Lerp(Random.onUnitSphere, axis, config.axisBias);
        return direction.sqrMagnitude > 0.0001f ? direction.normalized : axis;
    }

    private void TrySpawn()
    {
        GameObject prefab = config.asteroidPrefabs[Random.Range(0, config.asteroidPrefabs.Length)];
        float scale = Random.Range(config.scale.x, config.scale.y);
        float radius = AsteroidSpawn.GetRadius(prefab) * scale;
        Vector3 halfSize = config.size * 0.5f;

        for (int attempt = 0; attempt < 20; attempt++)
        {
            Vector3 candidate = new Vector3(
                Random.Range(-halfSize.x, halfSize.x),
                Random.Range(-halfSize.y, halfSize.y),
                Random.Range(-halfSize.z, halfSize.z));
            if (!IsFree(candidate, radius)) continue;

            placed.Add(new Vector4(candidate.x, candidate.y, candidate.z, radius));
            Rigidbody body = AsteroidSpawn.Create(prefab, transform.TransformPoint(candidate), Random.rotation, scale, config.density, transform);
            body.linearDamping = config.damping;
            body.angularVelocity = Random.onUnitSphere * Random.Range(config.spinSpeed.x, config.spinSpeed.y);

            Renderer renderer = body.GetComponentInChildren<Renderer>();
            asteroids.Add(new Asteroid
            {
                Body = body,
                Renderer = renderer,
                BaseMaterial = renderer != null ? renderer.sharedMaterial : null,
                // Staggered first impulses, so the stream does not start in one synchronized burst.
                NextImpulseTime = Time.time + Random.Range(0f, config.impulseInterval.y),
            });
            return;
        }
    }

    private bool IsFree(Vector3 point, float radius)
    {
        if (point.magnitude - radius < config.coreClearance) return false;

        foreach (Vector4 other in placed)
        {
            float minDistance = radius + other.w;
            if (((Vector3)other - point).sqrMagnitude < minDistance * minDistance) return false;
        }
        return true;
    }

    /// <summary>One emissive copy per asteroid material, shared by every glowing asteroid that uses it.</summary>
    private Material GetGlowMaterial(Material baseMaterial)
    {
        if (baseMaterial == null) return null;
        if (glowMaterials.TryGetValue(baseMaterial, out Material glow)) return glow;

        glow = new Material(baseMaterial) { name = baseMaterial.name + " (Telegraph)" };
        glow.EnableKeyword("_EMISSION");
        glow.SetColor("_EmissionColor", config.telegraphColor);
        glowMaterials.Add(baseMaterial, glow);
        return glow;
    }

    private void OnDrawGizmosSelected()
    {
        if (config == null) return;

        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.color = new Color(1f, 0.45f, 0.1f);
        Gizmos.DrawWireCube(Vector3.zero, config.size);
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(Vector3.zero, config.coreClearance);
    }
}
