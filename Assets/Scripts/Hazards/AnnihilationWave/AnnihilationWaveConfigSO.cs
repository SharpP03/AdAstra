using System;
using UnityEngine;

/// <summary>
/// ScriptableObject configuration asset for the Annihilation Wave.
/// Centralizes all key designer/balance settings (speed, dimensions, frequencies, visual parameters)
/// so they can be conveniently tuned in the Inspector without digging through child objects or materials.
/// </summary>
[CreateAssetMenu(fileName = "AnnihilationWave_Config", menuName = "AdAstra/Hazards/Annihilation Wave Config", order = 10)]
public class AnnihilationWaveConfigSO : ScriptableObject
{
    [Header("Wave Movement & Threat Distances")]
    [Tooltip("Forward movement speed of the wave in meters per second.")]
    [Range(0f, 60f)]
    public float speed = 12f;

    [Tooltip("Distance to player in meters at which the warning alert triggers.")]
    [Range(20f, 300f)]
    public float warningDistance = 100f;

    [Tooltip("Distance to player in meters at which the critical alert triggers.")]
    [Range(5f, 100f)]
    public float criticalDistance = 30f;

    [Tooltip("Continuous damage per second inflicted on objects engulfed by the wave.")]
    [Range(0f, 200f)]
    public float damagePerSecond = 50f;

    [Header("Storm Lightning Discharges (Obszar, Prędkość i Częstotliwość Piorunów)")]
    [Tooltip("If true, only one single lightning strike occurs at a time, ensuring majestic isolated flashes.")]
    public bool singleDischargeOnly = true;

    [Tooltip("Playback speed multiplier for lightning particles. Lower values (e.g. 0.3 - 0.6) slow down the lightning arc and flash.")]
    [Range(0.1f, 2f)]
    public float lightningSimulationSpeed = 0.5f;

    [Tooltip("How long in seconds each lightning flash stays active before deactivating.")]
    [Range(0.5f, 5f)]
    public float dischargeLifetime = 2.0f;

    [Tooltip("Width of the discharge spawn area across the wave front (X axis in meters).")]
    [Range(100f, 1000f)]
    public float dischargeFrontWidth = 420f;

    [Tooltip("Height of the discharge spawn area across the wave front (Y axis in meters).")]
    [Range(100f, 800f)]
    public float dischargeFrontHeight = 350f;

    [Tooltip("Minimum time interval in seconds between lightning strikes (np. 2.5s).")]
    [Range(0.5f, 15f)]
    public float minDischargeInterval = 2.5f;

    [Tooltip("Maximum time interval in seconds between lightning strikes (np. 5.5s).")]
    [Range(1f, 25f)]
    public float maxDischargeInterval = 5.5f;

    [Tooltip("Scale range (min, max) for spawned lightning discharge effects.")]
    public Vector2 dischargeScaleRange = new Vector2(25f, 50f);

    [Header("Proximity Sparks (Iskry Bliskości)")]
    [Tooltip("Width and height of the spark emitter box (X and Y in meters).")]
    public Vector2 sparkEmitterSize = new Vector2(140f, 140f);

    [Tooltip("Maximum emission rate for proximity sparks when engulfed or at critical distance.")]
    [Range(10f, 300f)]
    public float maxParticleEmissionRate = 60f;

    [Header("Shader Motion & Visuals (Prędkość i Wygląd Plazmy)")]
    [Tooltip("Multiplier for shader plasma drift speed. Lower = slower, calmer motion.")]
    [Range(0.1f, 5f)]
    public float shaderSpeedMultiplier = 1f;

    [Tooltip("Frequency of global wave energy pulsation.")]
    [Range(0.1f, 4f)]
    public float pulseSpeed = 0.6f;

    [Tooltip("Density/scale of the electrical filament web. Higher = finer, denser mesh.")]
    [Range(1f, 15f)]
    public float filamentWebScale = 5.5f;

    [Tooltip("Sharpness and thickness of the electrical filament arcs.")]
    [Range(1f, 8f)]
    public float filamentSharpness = 3.5f;

    [Tooltip("Degree of raggedness and erosion along the outer perimeter (masks square borders).")]
    [Range(0f, 1f)]
    public float edgeRaggedness = 0.45f;

    /// <summary>
    /// Event invoked in editor whenever any value is changed in the Inspector.
    /// </summary>
    public event Action OnConfigChanged;

    private void OnValidate()
    {
        speed = Mathf.Max(0f, speed);
        warningDistance = Mathf.Max(0f, warningDistance);
        criticalDistance = Mathf.Clamp(criticalDistance, 0f, warningDistance);
        damagePerSecond = Mathf.Max(0f, damagePerSecond);
        minDischargeInterval = Mathf.Max(0.01f, minDischargeInterval);
        maxDischargeInterval = Mathf.Max(minDischargeInterval, maxDischargeInterval);

        OnConfigChanged?.Invoke();
    }
}
