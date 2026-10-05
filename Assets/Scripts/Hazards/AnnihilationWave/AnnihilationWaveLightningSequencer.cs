using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages the GC-free object pool and sequencing of storm lightning strikes across the Annihilation Wave front.
/// Cadence dynamically escalates based on the player's threat state received from AnnihilationWaveController.
/// </summary>
[SelectionBase]
public class AnnihilationWaveLightningSequencer : MonoBehaviour
{
    [Header("Dependencies")]
    [Tooltip("Reference to the core wave controller. If null, resolved automatically.")]
    [SerializeField] private AnnihilationWaveController controller;

    [Tooltip("Configuration asset for balance parameters (intervals, scale, dimensions).")]
    [SerializeField] private AnnihilationWaveConfigSO config;

    [Tooltip("Parent transform under which the pooled lightning instances are instantiated.")]
    [SerializeField] private Transform poolParent;

    [Header("Pool Setup")]
    [Tooltip("Prefabs of electric storm lightning discharges spawned randomly across the wave front.")]
    [SerializeField] private GameObject[] stormDischargePrefabs;

    [Tooltip("Number of pooled lightning effect instances to avoid runtime allocations.")]
    [SerializeField] private int dischargePoolSize = 12;

    private class PooledDischarge
    {
        public GameObject gameObject;
        public Transform transform;
        public ParticleSystem[] particleSystems;
        public float deactivateTime;
    }

    private readonly List<PooledDischarge> dischargePool = new List<PooledDischarge>();
    private int dischargePoolIndex = 0;
    private Coroutine dischargeCoroutine;
    private WaveThreatState currentThreatState = WaveThreatState.Safe;

    private void Awake()
    {
        if (controller == null)
        {
            controller = GetComponentInParent<AnnihilationWaveController>();
        }

        if (config == null && controller != null)
        {
            config = controller.Config;
        }

        if (poolParent == null)
        {
            var follower = transform.Find("Visual_Follower");
            poolParent = follower != null ? follower : transform;
        }
    }

    private void OnEnable()
    {
        if (controller == null)
        {
            controller = GetComponentInParent<AnnihilationWaveController>();
        }

        if (config == null && controller != null)
        {
            config = controller.Config;
        }

        if (controller != null)
        {
            controller.OnThreatStateChanged += HandleThreatStateChanged;
            currentThreatState = controller.CurrentThreatState;
        }

        if (Application.isPlaying)
        {
            if (dischargePool.Count > 0 && dischargeCoroutine == null)
            {
                dischargeCoroutine = StartCoroutine(StormDischargeLoop());
            }
        }
    }

    private void OnDisable()
    {
        if (controller != null)
        {
            controller.OnThreatStateChanged -= HandleThreatStateChanged;
        }

        if (dischargeCoroutine != null)
        {
            StopCoroutine(dischargeCoroutine);
            dischargeCoroutine = null;
        }
    }

    private void Start()
    {
        if (!Application.isPlaying) return;

        InitializeStormDischargePool();

        if (dischargeCoroutine == null && dischargePool.Count > 0)
        {
            dischargeCoroutine = StartCoroutine(StormDischargeLoop());
        }
    }

    private void LateUpdate()
    {
        if (!Application.isPlaying) return;

        UpdateDischargeTimers();
    }

    private void HandleThreatStateChanged(WaveThreatState state)
    {
        currentThreatState = state;
    }

    /// <summary>
    /// Pre-instantiates a pool of lightning discharge effects and caches particle components to guarantee GC-free execution.
    /// </summary>
    public void InitializeStormDischargePool()
    {
        GameObject[] prefabsToUse = stormDischargePrefabs;
        if ((prefabsToUse == null || prefabsToUse.Length == 0) && controller != null)
        {
            prefabsToUse = controller.StormDischargePrefabs;
        }

        if (prefabsToUse == null || prefabsToUse.Length == 0) return;

        Transform container = poolParent != null ? poolParent : transform;
        Transform poolContainer = container.Find("Storm_Discharges");
        if (poolContainer == null)
        {
            var go = new GameObject("Storm_Discharges");
            go.transform.SetParent(container, false);
            poolContainer = go.transform;
        }

        dischargePool.Clear();
        for (int i = 0; i < dischargePoolSize; i++)
        {
            GameObject prefab = prefabsToUse[i % prefabsToUse.Length];
            if (prefab == null) continue;

            GameObject instance = Instantiate(prefab, poolContainer);
            instance.name = $"Discharge_Pooled_{i}";
            instance.SetActive(false);

            dischargePool.Add(new PooledDischarge
            {
                gameObject = instance,
                transform = instance.transform,
                particleSystems = instance.GetComponentsInChildren<ParticleSystem>(true),
                deactivateTime = -1f
            });
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
            if (currentThreatState == WaveThreatState.Warning)
            {
                threatMultiplier = 0.65f;
            }
            else if (currentThreatState == WaveThreatState.Critical || currentThreatState == WaveThreatState.Engulfed)
            {
                threatMultiplier = 0.35f;
            }

            float minInterval = config != null ? config.minDischargeInterval : 2.5f;
            float maxInterval = config != null ? config.maxDischargeInterval : 5.5f;
            float waitTime = UnityEngine.Random.Range(minInterval, maxInterval) * threatMultiplier;
            yield return new WaitForSeconds(waitTime);

            TriggerRandomDischarge();
        }
    }

    /// <summary>
    /// Activates a pooled lightning effect at a randomized position across the front of the wave (GC-Free).
    /// </summary>
    public void TriggerRandomDischarge()
    {
        if (dischargePool.Count == 0) return;

        PooledDischarge item = dischargePool[dischargePoolIndex];
        dischargePoolIndex = (dischargePoolIndex + 1) % dischargePool.Count;

        if (item == null || item.gameObject == null) return;

        float frontW = config != null ? config.dischargeFrontWidth : 420f;
        float frontH = config != null ? config.dischargeFrontHeight : 350f;
        float halfW = frontW * 0.5f;
        float halfH = frontH * 0.5f;
        float x = UnityEngine.Random.Range(-halfW, halfW);
        float y = UnityEngine.Random.Range(-halfH, halfH);
        float z = UnityEngine.Random.Range(-1f, 1f);

        bool singleOnly = config != null ? config.singleDischargeOnly : true;
        if (singleOnly)
        {
            for (int i = 0; i < dischargePool.Count; i++)
            {
                var other = dischargePool[i];
                if (other != null && other.gameObject != null && other.gameObject.activeSelf)
                {
                    other.gameObject.SetActive(false);
                    other.deactivateTime = -1f;
                }
            }
        }

        item.transform.localPosition = new Vector3(x, y, z);
        item.transform.localRotation = Quaternion.Euler(0f, 0f, UnityEngine.Random.Range(0f, 360f));
        Vector2 scaleRange = config != null ? config.dischargeScaleRange : new Vector2(25f, 50f);
        float scale = UnityEngine.Random.Range(scaleRange.x, scaleRange.y);
        item.transform.localScale = Vector3.one * scale;

        item.gameObject.SetActive(true);

        float simSpeed = config != null ? config.lightningSimulationSpeed : 0.5f;
        if (item.particleSystems != null)
        {
            for (int i = 0; i < item.particleSystems.Length; i++)
            {
                var ps = item.particleSystems[i];
                if (ps != null)
                {
                    var main = ps.main;
                    main.simulationSpeed = simSpeed;
                    ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                    ps.Play(true);
                }
            }
        }

        float lifetime = config != null ? config.dischargeLifetime : 2f;
        item.deactivateTime = Time.time + lifetime;
    }

    /// <summary>
    /// Updates pooled discharge timers in LateUpdate, deactivating expired instances without coroutine allocations.
    /// </summary>
    private void UpdateDischargeTimers()
    {
        if (dischargePool.Count == 0) return;

        float now = Time.time;
        for (int i = 0; i < dischargePool.Count; i++)
        {
            var item = dischargePool[i];
            if (item != null && item.deactivateTime > 0f && now >= item.deactivateTime)
            {
                item.deactivateTime = -1f;
                if (item.gameObject != null && item.gameObject.activeSelf)
                {
                    item.gameObject.SetActive(false);
                }
            }
        }
    }
}
