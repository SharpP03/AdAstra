using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>
/// Asteroid field that guards a Stasis Core in a small pocket at its center. Asteroids gather in clusters
/// around big boulders, more of them toward the core, and breathe around their anchors in rhythm with the
/// core's pulse, so the gaps between them open and close. Once the core is taken, the field closes in.
/// </summary>
public class AsteroidCoreField : MonoBehaviour
{
    [SerializeField] private AsteroidCoreFieldConfigSO config;
    [Tooltip("Core in the pocket. Optional: without it the field breathes without the pulse and never collapses.")]
    [SerializeField] private StasisCore core;

    private struct Asteroid
    {
        public Rigidbody Body;
        public Vector3 Anchor;      // local position the asteroid is held around
        public float Closeness;     // 0 at the edge of the field, 1 at the pocket
        public Vector3 NoiseOffset; // separate noise sample per asteroid and axis
    }

    private readonly List<Asteroid> asteroids = new List<Asteroid>();
    private readonly List<Vector4> placed = new List<Vector4>(); // xyz = local anchor, w = radius
    private float collapseStartTime = -1f;

    private void OnEnable()
    {
        if (core != null) core.OnCollected += HandleCoreCollected;
    }

    private void OnDisable()
    {
        if (core != null) core.OnCollected -= HandleCoreCollected;
    }

    private void Start()
    {
        if (config == null || config.asteroidPrefabs == null || config.asteroidPrefabs.Length == 0)
        {
            Debug.LogError("AsteroidCoreField: missing config or asteroid prefabs.", this);
            enabled = false;
            return;
        }

        // A fixed seed must not change the random sequence of other systems.
        Random.State previousState = Random.state;
        if (config.seed != 0) Random.InitState(config.seed);

        for (int i = 0; i < config.clusterCount; i++) SpawnCluster();
        for (int i = 0; i < config.looseCount; i++)
            TrySpawn(() => RandomPointInField(config.pocketRadius, config.centerBias), MemberScale());

        if (config.seed != 0) Random.state = previousState;
    }

    private void FixedUpdate()
    {
        #region EDUCATION NOTE breathing asteroids
        // DO PRACY INŻ. - asteroida "na sprężynie" zamiast swobodnego dryfu.
        // Każda asteroida ma kotwicę, a cel ruchu to kotwica + wolny szum Perlina + promieniowy puls w rytm rdzenia.
        // Przyspieszenie a = k·(cel - pozycja) - c·prędkość to tłumiony oscylator harmoniczny:
        // k = ω² wynika z okresu sprężyny, a c = 2ζω z współczynnika tłumienia ζ (ζ = 1 - powrót bez przeregulowania).
        // Ciało pozostaje dynamiczne, więc zderzenie ze statkiem nadal liczy masę i obrażenia,
        // a po trąceniu asteroida sama wraca na miejsce - pole żyje, ale się nie rozpada.
        #endregion
        float omega = 2f * Mathf.PI / config.springPeriod;
        float stiffness = omega * omega;
        float damping = 2f * config.springDamping * omega;

        float time = Time.time / config.wanderPeriod;
        float pulse = core != null ? core.Pulse : 0f;
        float collapse = collapseStartTime < 0f
            ? 0f
            : Mathf.SmoothStep(0f, config.collapseFraction, (Time.time - collapseStartTime) / config.collapseDuration);

        foreach (Asteroid a in asteroids)
        {
            if (a.Body == null) continue;

            Vector3 anchor = a.Anchor * (1f - collapse);
            Vector3 wander = new Vector3(
                Mathf.PerlinNoise(a.NoiseOffset.x, time) - 0.5f,
                Mathf.PerlinNoise(a.NoiseOffset.y, time) - 0.5f,
                Mathf.PerlinNoise(a.NoiseOffset.z, time) - 0.5f) * (2f * Mathf.Lerp(config.wanderAmplitude.x, config.wanderAmplitude.y, a.Closeness));
            Vector3 radial = a.Anchor.normalized * (pulse * Mathf.Lerp(config.pulseAmplitude.x, config.pulseAmplitude.y, a.Closeness));

            Vector3 target = transform.TransformPoint(anchor + wander + radial);
            Vector3 acceleration = stiffness * (target - a.Body.position) - damping * a.Body.linearVelocity;
            a.Body.AddForce(acceleration, ForceMode.Acceleration);
        }
    }

    private void HandleCoreCollected()
    {
        collapseStartTime = Time.time;
    }

    private void SpawnCluster()
    {
        float clusterRadius = Random.Range(config.clusterRadius.x, config.clusterRadius.y);
        Vector3 center = RandomPointInField(config.pocketRadius + clusterRadius * 0.5f, config.centerBias);

        // The boulder gives the cluster its shape; members fill the space around it.
        TrySpawn(() => center + Random.insideUnitSphere * (clusterRadius * 0.3f), Random.Range(config.boulderScale.x, config.boulderScale.y));

        int members = Random.Range(config.clusterMembers.x, config.clusterMembers.y + 1);
        for (int i = 0; i < members; i++) TrySpawn(() => center + Random.insideUnitSphere * clusterRadius, MemberScale());
    }

    /// <summary>Many small asteroids and few big ones.</summary>
    private float MemberScale()
    {
        return Mathf.Lerp(config.memberScale.x, config.memberScale.y, Mathf.Pow(Random.value, config.sizeBias));
    }

    /// <summary>
    /// Random local point between <paramref name="minDistance"/> and the edge of the flattened field.
    /// <paramref name="bias"/> above 1 pulls the points toward the center.
    /// </summary>
    private Vector3 RandomPointInField(float minDistance, float bias)
    {
        #region EDUCATION NOTE radial density
        // DO PRACY INŻ. - gradient gęstości bez osobnej funkcji gęstości.
        // Odległość od środka losujemy równomiernie (r ~ U(a, b)), a nie równomiernie w objętości (r ~ U(a³, b³)^(1/3)).
        // Na każdą jednostkę promienia przypada wtedy tyle samo skupisk, a powierzchnia sfery rośnie jak r²,
        // więc gęstość maleje jak 1/r²: najciaśniej jest przy kieszeni z rdzeniem, a krawędź pola jest rzadka.
        // Wykładnik bias > 1 (r = a + (b - a)·u^bias) dodatkowo przesuwa skupiska ku środkowi.
        // Na koniec punkt jest skalowany per oś (shapeScale), co spłaszcza pole w dysk.
        #endregion
        float min = Mathf.Min(minDistance, config.fieldRadius);
        float distance = Mathf.Lerp(min, config.fieldRadius, Mathf.Pow(Random.value, bias));
        return Vector3.Scale(Random.onUnitSphere * distance, config.shapeScale);
    }

    /// <summary>Spawns one asteroid at the first free spot drawn from <paramref name="sampleSpot"/>, or skips it.</summary>
    private void TrySpawn(Func<Vector3> sampleSpot, float scale)
    {
        GameObject prefab = config.asteroidPrefabs[Random.Range(0, config.asteroidPrefabs.Length)];
        float radius = AsteroidSpawn.GetRadius(prefab) * scale;

        for (int attempt = 0; attempt < config.placementAttempts; attempt++)
        {
            Vector3 candidate = sampleSpot();
            if (!IsFree(candidate, radius)) continue;

            placed.Add(new Vector4(candidate.x, candidate.y, candidate.z, radius));
            Rigidbody body = AsteroidSpawn.Create(prefab, transform.TransformPoint(candidate), Random.rotation, scale, config.density, transform);
            body.angularVelocity = Random.onUnitSphere * Random.Range(config.spinSpeed.x, config.spinSpeed.y);

            asteroids.Add(new Asteroid
            {
                Body = body,
                Anchor = candidate,
                Closeness = 1f - Mathf.InverseLerp(config.pocketRadius, config.fieldRadius, candidate.magnitude),
                NoiseOffset = new Vector3(Random.value, Random.value, Random.value) * 1000f,
            });
            return;
        }
    }

    private bool IsFree(Vector3 point, float radius)
    {
        if (point.magnitude - radius < config.pocketRadius) return false;

        // The entrance runs from the pocket toward local -Z, so the core is visible from the approach side.
        bool inEntrance = point.z < 0f && new Vector2(point.x, point.y).magnitude < config.entranceRadius + radius;
        if (inEntrance) return false;

        foreach (Vector4 other in placed)
        {
            float minDistance = radius + other.w + config.minClearance;
            if (((Vector3)other - point).sqrMagnitude < minDistance * minDistance) return false;
        }
        return true;
    }

    private void OnDrawGizmosSelected()
    {
        if (config == null) return;

        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(Vector3.zero, config.pocketRadius);
        Gizmos.DrawLine(Vector3.zero, Vector3.back * config.fieldRadius * config.shapeScale.z);

        Gizmos.matrix = transform.localToWorldMatrix * Matrix4x4.Scale(config.shapeScale);
        Gizmos.color = Color.gray;
        Gizmos.DrawWireSphere(Vector3.zero, config.fieldRadius);
    }
}
