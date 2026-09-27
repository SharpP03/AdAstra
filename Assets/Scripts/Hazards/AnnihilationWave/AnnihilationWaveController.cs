using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum WaveThreatState
{
    Safe,       // Player is well ahead of the wave (> warningDistance)
    Warning,    // Player is within warning perimeter (<= warningDistance)
    Critical,   // Player is dangerously close (<= criticalDistance)
    Engulfed    // Player is inside / behind the wave boundary (<= 0)
}

/// <summary>
/// Controls the movement, distance tracking, visual centering, and destructive damage of the Annihilation Wave.
/// Features dynamic player X/Y visual centering and proximity particle surge.
/// </summary>
[SelectionBase]
[RequireComponent(typeof(Rigidbody))]
public class AnnihilationWaveController : MonoBehaviour
{
    [Header("Configuration Asset (Optional)")]
    [Tooltip("ScriptableObject configuration asset holding key balance and tuning parameters. If assigned, values from this asset override local defaults.")]
    [SerializeField] private AnnihilationWaveConfigSO config;

    [Tooltip("Material of the energy curtain. If null, resolved automatically from visualRoot/Energy_Curtain.")]
    [SerializeField] private Material frontMaterial;

    [Header("Movement")]
    [Tooltip("Movement speed along the forward axis in meters per second.")]
    [SerializeField] private float speed = 12f;

    [Tooltip("Whether the wave automatically starts moving on Start.")]
    [SerializeField] private bool autoStart = true;

    [Header("Perimeter & Threat Thresholds")]
    [Tooltip("Distance in meters at which the warning alert triggers.")]
    [SerializeField] private float warningDistance = 100f;

    [Tooltip("Distance in meters at which the critical warning alert triggers.")]
    [SerializeField] private float criticalDistance = 30f;

    [Header("Damage")]
    [Tooltip("Damage inflicted per second to any IDamageable inside the wave.")]
    [SerializeField] private float damagePerSecond = 50f;

    [Tooltip("If true, also inflicts damage to the target player whenever their position is behind the wave front plane.")]
    [SerializeField] private bool planeBoundaryDamage = true;

    [Header("Visual Centering (Infinite Horizon)")]
    [Tooltip("Transform of the visual container/plane to center on the player in X/Y coordinates.")]
    [SerializeField] private Transform visualRoot;

    [Tooltip("If true, keeps visualRoot centered on the player's X and Y coordinates so the wave appears infinite.")]
    [SerializeField] private bool followPlayerXY = true;

    [Header("Proximity Particle Surge")]
    [Tooltip("Particle system emitting energetic sparks/plasma towards the player when threatened.")]
    [SerializeField] private ParticleSystem proximityParticles;

    [Tooltip("Maximum emission rate when player is engulfed or at critical proximity.")]
    [SerializeField] private float maxParticleEmissionRate = 60f;

    [Header("Dynamic Threat Lighting")]
    [Tooltip("Point/Spot light on the wave front casting ominous illumination on the player ship and surroundings.")]
    [SerializeField] private Light threatLight;

    [Tooltip("Base intensity for the threat light.")]
    [SerializeField] private float baseLightIntensity = 2.5f;

    [Tooltip("Extra light intensity boost when player is in proximity of the wave.")]
    [SerializeField] private float maxLightIntensityBoost = 5f;

    [Tooltip("Flicker frequency for the pulsating light.")]
    [SerializeField] private float lightFlickerFrequency = 3.5f;

    [Header("Storm Discharges")]
    [Tooltip("Prefabs of electric storm lightning discharges spawned randomly across the wave front.")]
    [SerializeField] private GameObject[] stormDischargePrefabs;

    [Tooltip("If true, only a single lightning strike occurs at a time, ensuring isolated majestic flashes.")]
    [SerializeField] private bool singleDischargeOnly = true;

    [Tooltip("Playback speed multiplier for lightning particles. Lower values slow down the lightning arc and flash.")]
    [SerializeField] private float lightningSimulationSpeed = 0.5f;

    [Tooltip("How long in seconds each lightning flash stays active before deactivating.")]
    [SerializeField] private float dischargeLifetime = 2.0f;

    [Tooltip("Width of the discharge spawn area across the wave front (X axis).")]
    [SerializeField] private float dischargeFrontWidth = 420f;

    [Tooltip("Height of the discharge spawn area across the wave front (Y axis).")]
    [SerializeField] private float dischargeFrontHeight = 350f;

    [Tooltip("Minimum time interval in seconds between lightning discharges.")]
    [SerializeField] private float minDischargeInterval = 2.5f;

    [Tooltip("Maximum time interval in seconds between lightning discharges.")]
    [SerializeField] private float maxDischargeInterval = 5.5f;

    [Tooltip("Scale range for spawned lightning discharges.")]
    [SerializeField] private Vector2 dischargeScaleRange = new Vector2(25f, 50f);

    [Tooltip("Number of pooled lightning effect instances to avoid runtime allocations.")]
    [SerializeField] private int dischargePoolSize = 12;

    private readonly List<GameObject> dischargePool = new List<GameObject>();
    private int dischargePoolIndex = 0;
    private Coroutine dischargeCoroutine;

    [Header("Target Tracking")]
    [Tooltip("Target transform to track (usually the player's spaceship). If null, resolves from GameManager.")]
    [SerializeField] private Transform targetPlayer;

    [Header("Optional Collider")]
    [Tooltip("Optional trigger collider used for volume-based contact damage.")]
    [SerializeField] private Collider waveTrigger;

    // Public properties (Deep Module interface)
    public AnnihilationWaveConfigSO Config => config;
    public float Speed => speed;
    public bool IsActive { get; private set; }
    public float CurrentDistanceToPlayer { get; private set; } = float.MaxValue;
    public WaveThreatState CurrentThreatState { get; private set; } = WaveThreatState.Safe;

    // Events for UI/HUD and game systems
    public event Action<float> OnDistanceChanged;
    public event Action<WaveThreatState> OnThreatStateChanged;

    private Rigidbody rb;
    private IDamageable cachedPlayerDamageable;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        if (waveTrigger == null)
        {
            waveTrigger = GetComponent<Collider>();
        }

        if (waveTrigger != null && !waveTrigger.isTrigger)
        {
            waveTrigger.isTrigger = true;
        }

        if (visualRoot == null)
        {
            var follower = transform.Find("Visual_Follower");
            if (follower != null)
            {
                visualRoot = follower;
            }
            else
            {
                var child = transform.Find("Visual_Plane");
                if (child != null) visualRoot = child;
            }
        }

        if (proximityParticles == null)
        {
            proximityParticles = GetComponentInChildren<ParticleSystem>();
        }

        if (threatLight == null)
        {
            threatLight = GetComponentInChildren<Light>();
        }

        ApplyConfiguration();
    }

    private void OnEnable()
    {
        if (config != null)
        {
            config.OnConfigChanged += ApplyConfiguration;
        }

        if (dischargeCoroutine == null && stormDischargePrefabs != null && stormDischargePrefabs.Length > 0)
        {
            dischargeCoroutine = StartCoroutine(StormDischargeLoop());
        }
    }

    private void OnDisable()
    {
        if (config != null)
        {
            config.OnConfigChanged -= ApplyConfiguration;
        }

        if (dischargeCoroutine != null)
        {
            StopCoroutine(dischargeCoroutine);
            dischargeCoroutine = null;
        }
    }

    private void Start()
    {
        ResolveTargetPlayer();
        InitializeStormDischargePool();

        if (dischargeCoroutine == null && stormDischargePrefabs != null && stormDischargePrefabs.Length > 0)
        {
            dischargeCoroutine = StartCoroutine(StormDischargeLoop());
        }

        if (autoStart)
        {
            IsActive = true;
        }
    }

    private void FixedUpdate()
    {
        if (IsActive)
        {
            MoveWave();
        }

        UpdateThreatTracking();
        HandlePlaneBoundaryDamage();
    }

    private void LateUpdate()
    {
        UpdateVisualCentering();
        UpdateParticleSurge();
        UpdateThreatLight();
    }

    /// <summary>
    /// Advances the wave forward using Rigidbody kinematic movement.
    /// </summary>
    private void MoveWave()
    {
        Vector3 delta = transform.forward * (speed * Time.fixedDeltaTime);
        if (rb != null)
        {
            rb.MovePosition(rb.position + delta);
        }
        else
        {
            transform.position += delta;
        }
    }

    /// <summary>
    /// Centers the visual plane and particle source on the player's X and Y coordinates.
    /// Eliminates visible side edges and prevents flying around the visual boundary in open 3D space.
    /// </summary>
    private void UpdateVisualCentering()
    {
        if (!followPlayerXY || targetPlayer == null || visualRoot == null) return;

        // Visual plane moves with the wave along Z, but matches the player's X and Y
        Vector3 targetPos = new Vector3(targetPlayer.position.x, targetPlayer.position.y, transform.position.z);
        visualRoot.position = targetPos;
    }

    /// <summary>
    /// Modulates proximity particle intensity according to distance to player.
    /// </summary>
    private void UpdateParticleSurge()
    {
        if (proximityParticles == null) return;

        float targetRate = 0f;

        if (CurrentThreatState == WaveThreatState.Engulfed)
        {
            targetRate = maxParticleEmissionRate * 1.5f;
        }
        else if (CurrentThreatState == WaveThreatState.Critical || CurrentThreatState == WaveThreatState.Warning)
        {
            float factor = 1f - Mathf.Clamp01(CurrentDistanceToPlayer / warningDistance);
            targetRate = factor * maxParticleEmissionRate;
        }

        var emission = proximityParticles.emission;
        emission.rateOverTime = targetRate;

        if (targetRate > 0.01f && !proximityParticles.isPlaying)
        {
            proximityParticles.Play();
        }
        else if (targetRate <= 0.01f && proximityParticles.isPlaying && proximityParticles.particleCount == 0)
        {
            proximityParticles.Stop();
        }
    }

    /// <summary>
    /// Modulates ominous threat light intensity and subtle flickering as the wave approaches the player.
    /// </summary>
    private void UpdateThreatLight()
    {
        if (threatLight == null) return;

        float proximity = (CurrentDistanceToPlayer <= warningDistance && CurrentDistanceToPlayer >= 0f)
            ? (1f - (CurrentDistanceToPlayer / warningDistance))
            : (CurrentThreatState == WaveThreatState.Engulfed ? 1.5f : 0.1f);

        float flicker = Mathf.Sin(Time.time * lightFlickerFrequency) * 0.3f
                      + Mathf.Cos(Time.time * (lightFlickerFrequency * 1.6f)) * 0.15f;

        threatLight.intensity = Mathf.Max(0.5f, baseLightIntensity + (proximity * maxLightIntensityBoost) + (flicker * (1f + proximity)));
    }

    /// <summary>
    /// Pre-instantiates a pool of lightning discharge effects under visualRoot to avoid runtime GC spikes.
    /// </summary>
    private void InitializeStormDischargePool()
    {
        if (stormDischargePrefabs == null || stormDischargePrefabs.Length == 0) return;

        Transform parentTransform = visualRoot != null ? visualRoot : transform;
        Transform poolContainer = parentTransform.Find("Storm_Discharges");
        if (poolContainer == null)
        {
            var go = new GameObject("Storm_Discharges");
            go.transform.SetParent(parentTransform, false);
            poolContainer = go.transform;
        }

        dischargePool.Clear();
        for (int i = 0; i < dischargePoolSize; i++)
        {
            GameObject prefab = stormDischargePrefabs[i % stormDischargePrefabs.Length];
            if (prefab == null) continue;

            GameObject instance = Instantiate(prefab, poolContainer);
            instance.name = $"Discharge_Pooled_{i}";
            instance.SetActive(false);
            dischargePool.Add(instance);
        }
    }

    /// <summary>
    /// Sequencer loop triggering random storm lightning strikes across the wave front.
    /// Frequency escalates as the player enters warning and critical perimeters.
    /// </summary>
    private IEnumerator StormDischargeLoop()
    {
        while (true)
        {
            float threatMultiplier = 1f;
            if (CurrentThreatState == WaveThreatState.Warning)
            {
                threatMultiplier = 0.65f;
            }
            else if (CurrentThreatState == WaveThreatState.Critical || CurrentThreatState == WaveThreatState.Engulfed)
            {
                threatMultiplier = 0.35f;
            }

            float waitTime = UnityEngine.Random.Range(minDischargeInterval, maxDischargeInterval) * threatMultiplier;
            yield return new WaitForSeconds(waitTime);

            TriggerRandomDischarge();
        }
    }

    /// <summary>
    /// Activates a pooled lightning effect at a randomized position across the front of the wave.
    /// </summary>
    private void TriggerRandomDischarge()
    {
        if (dischargePool.Count == 0) return;

        GameObject instance = dischargePool[dischargePoolIndex];
        dischargePoolIndex = (dischargePoolIndex + 1) % dischargePool.Count;

        if (instance == null) return;

        float halfW = dischargeFrontWidth * 0.5f;
        float halfH = dischargeFrontHeight * 0.5f;
        float x = UnityEngine.Random.Range(-halfW, halfW);
        float y = UnityEngine.Random.Range(-halfH, halfH);
        float z = UnityEngine.Random.Range(-1f, 1f);

        if (singleDischargeOnly)
        {
            for (int i = 0; i < dischargePool.Count; i++)
            {
                if (dischargePool[i] != null && dischargePool[i].activeSelf)
                {
                    dischargePool[i].SetActive(false);
                }
            }
        }

        instance.transform.localPosition = new Vector3(x, y, z);
        instance.transform.localRotation = Quaternion.Euler(0f, 0f, UnityEngine.Random.Range(0f, 360f));
        float scale = UnityEngine.Random.Range(dischargeScaleRange.x, dischargeScaleRange.y);
        instance.transform.localScale = Vector3.one * scale;

        instance.SetActive(false);
        instance.SetActive(true);

        ParticleSystem[] particleSystems = instance.GetComponentsInChildren<ParticleSystem>();
        for (int i = 0; i < particleSystems.Length; i++)
        {
            var main = particleSystems[i].main;
            main.simulationSpeed = lightningSimulationSpeed;
            particleSystems[i].Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            particleSystems[i].Play(true);
        }

        StartCoroutine(DeactivateDischargeAfterDelay(instance, dischargeLifetime));
    }

    private IEnumerator DeactivateDischargeAfterDelay(GameObject instance, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (instance != null)
        {
            instance.SetActive(false);
        }
    }

    /// <summary>
    /// Calculates the signed distance to the player along the wave's forward axis and updates threat state.
    /// </summary>
    private void UpdateThreatTracking()
    {
        if (targetPlayer == null)
        {
            ResolveTargetPlayer();
            if (targetPlayer == null) return;
        }

        Vector3 toPlayer = targetPlayer.position - transform.position;
        float distance = Vector3.Dot(toPlayer, transform.forward);

        CurrentDistanceToPlayer = distance;
        OnDistanceChanged?.Invoke(distance);

        WaveThreatState newState;
        if (distance <= 0f)
        {
            newState = WaveThreatState.Engulfed;
        }
        else if (distance <= criticalDistance)
        {
            newState = WaveThreatState.Critical;
        }
        else if (distance <= warningDistance)
        {
            newState = WaveThreatState.Warning;
        }
        else
        {
            newState = WaveThreatState.Safe;
        }

        if (newState != CurrentThreatState)
        {
            CurrentThreatState = newState;
            OnThreatStateChanged?.Invoke(newState);
        }
    }

    /// <summary>
    /// Applies continuous damage to the tracked player if behind the wave plane.
    /// </summary>
    private void HandlePlaneBoundaryDamage()
    {
        if (!planeBoundaryDamage || CurrentThreatState != WaveThreatState.Engulfed) return;

        if (cachedPlayerDamageable != null)
        {
            cachedPlayerDamageable.TakeDamage(damagePerSecond * Time.fixedDeltaTime);
        }
    }

    /// <summary>
    /// Triggers damage over time to any IDamageable object inside the wave trigger volume.
    /// </summary>
    private void OnTriggerStay(Collider other)
    {
        // If plane boundary damage already handled the tracked player, avoid double damage
        if (planeBoundaryDamage && targetPlayer != null && other.transform.root == targetPlayer.root)
        {
            return;
        }

        if (other.TryGetComponent<IDamageable>(out var damageable))
        {
            damageable.TakeDamage(damagePerSecond * Time.fixedDeltaTime);
        }
        else
        {
            var parentDamageable = other.GetComponentInParent<IDamageable>();
            parentDamageable?.TakeDamage(damagePerSecond * Time.fixedDeltaTime);
        }
    }

    /// <summary>
    /// Resolves the player reference via serialized field or GameManager.
    /// </summary>
    private void ResolveTargetPlayer()
    {
        if (targetPlayer == null && GameManager.Instance != null && GameManager.Instance.Player != null)
        {
            targetPlayer = GameManager.Instance.Player.transform;
        }

        if (targetPlayer != null && cachedPlayerDamageable == null)
        {
            cachedPlayerDamageable = targetPlayer.GetComponentInParent<IDamageable>();
        }
    }

    /// <summary>
    /// Injects target player reference manually (e.g. for testing or dynamic spawner).
    /// </summary>
    public void SetTarget(Transform playerTransform)
    {
        targetPlayer = playerTransform;
        cachedPlayerDamageable = playerTransform != null ? playerTransform.GetComponentInParent<IDamageable>() : null;
    }

    public void SetSpeed(float newSpeed)
    {
        speed = Mathf.Max(0f, newSpeed);
    }

    public void SetActive(bool active)
    {
        IsActive = active;
    }

    private void OnValidate()
    {
        speed = Mathf.Max(0f, speed);
        warningDistance = Mathf.Max(0f, warningDistance);
        criticalDistance = Mathf.Clamp(criticalDistance, 0f, warningDistance);
        damagePerSecond = Mathf.Max(0f, damagePerSecond);

        ApplyConfiguration();
    }

    /// <summary>
    /// Synchronizes values from the assigned AnnihilationWaveConfigSO asset to runtime fields,
    /// child particle systems, and the front shader material.
    /// </summary>
    public void ApplyConfiguration()
    {
        if (config == null) return;

        speed = config.speed;
        warningDistance = config.warningDistance;
        criticalDistance = config.criticalDistance;
        damagePerSecond = config.damagePerSecond;
        dischargeFrontWidth = config.dischargeFrontWidth;
        dischargeFrontHeight = config.dischargeFrontHeight;
        singleDischargeOnly = config.singleDischargeOnly;
        lightningSimulationSpeed = config.lightningSimulationSpeed;
        dischargeLifetime = config.dischargeLifetime;
        minDischargeInterval = config.minDischargeInterval;
        maxDischargeInterval = config.maxDischargeInterval;
        dischargeScaleRange = config.dischargeScaleRange;
        maxParticleEmissionRate = config.maxParticleEmissionRate;

        if (proximityParticles == null && visualRoot != null)
        {
            var pChild = visualRoot.Find("Proximity_Particles");
            if (pChild != null) proximityParticles = pChild.GetComponent<ParticleSystem>();
        }

        if (proximityParticles != null)
        {
            var shape = proximityParticles.shape;
            if (shape.shapeType == ParticleSystemShapeType.Box)
            {
                shape.scale = new Vector3(config.sparkEmitterSize.x, config.sparkEmitterSize.y, shape.scale.z);
            }
        }

        if (frontMaterial == null && visualRoot != null)
        {
            var curtain = visualRoot.Find("Energy_Curtain");
            if (curtain != null)
            {
                var mr = curtain.GetComponent<MeshRenderer>();
                if (mr != null) frontMaterial = mr.sharedMaterial;
            }
        }

        if (frontMaterial != null)
        {
            Vector4 baseSpeed1 = new Vector4(0.006f, 0.012f, 0f, 0f) * config.shaderSpeedMultiplier;
            Vector4 baseSpeed2 = new Vector4(-0.009f, 0.007f, 0f, 0f) * config.shaderSpeedMultiplier;
            frontMaterial.SetVector("_Speed1", baseSpeed1);
            frontMaterial.SetVector("_Speed2", baseSpeed2);
            frontMaterial.SetFloat("_PulseSpeed", config.pulseSpeed);
            frontMaterial.SetFloat("_VoronoiScale", config.filamentWebScale);
            frontMaterial.SetFloat("_VoronoiPower", config.filamentSharpness);
            frontMaterial.SetFloat("_EdgeNoiseDistortion", config.edgeRaggedness);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Vector3 center = transform.position;
        Vector3 forward = transform.forward;
        Vector3 right = transform.right * 100f;
        Vector3 up = transform.up * 100f;

        // Draw wave front plane
        Gizmos.DrawLine(center - right - up, center + right - up);
        Gizmos.DrawLine(center + right - up, center + right + up);
        Gizmos.DrawLine(center + right + up, center - right + up);
        Gizmos.DrawLine(center - right + up, center - right - up);

        // Draw critical distance plane
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.7f);
        Vector3 critCenter = center + forward * criticalDistance;
        Gizmos.DrawLine(critCenter - right, critCenter + right);

        // Draw warning distance plane
        Gizmos.color = new Color(1f, 0.92f, 0.016f, 0.5f);
        Vector3 warnCenter = center + forward * warningDistance;
        Gizmos.DrawLine(warnCenter - right, warnCenter + right);

        // Forward vector
        Gizmos.color = Color.cyan;
        Gizmos.DrawRay(center, forward * 20f);
    }
}
