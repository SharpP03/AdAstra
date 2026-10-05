using System;
using UnityEngine;

/// <summary>
/// Handles sensory and visual presentation for the Annihilation Wave:
/// - Player X/Y centering for infinite visual horizon
/// - Dynamic MaterialPropertyBlock updates for the plasma curtain shader
/// - Threat proximity particle surge modulation
/// - Dynamic pulsating threat lighting
/// </summary>
[ExecuteAlways]
[SelectionBase]
public class AnnihilationWaveVisuals : MonoBehaviour
{
    [Header("Dependencies")]
    [Tooltip("Reference to the core wave controller. If null, resolved automatically from this GameObject.")]
    [SerializeField] private AnnihilationWaveController controller;

    [Tooltip("Configuration asset for shader and particle balance settings.")]
    [SerializeField] private AnnihilationWaveConfigSO config;

    [Header("Visual Horizon Centering")]
    [Tooltip("Transform of the visual container/plane to center on the player in X/Y coordinates.")]
    [SerializeField] private Transform visualRoot;

    [Tooltip("If true, keeps visualRoot centered on the player's X and Y coordinates so the wave appears infinite.")]
    [SerializeField] private bool followPlayerXY = true;

    [Header("Curtain Shader (MaterialPropertyBlock)")]
    [Tooltip("Renderer of the energy curtain. If null, resolved automatically from visualRoot/Energy_Curtain.")]
    [SerializeField] private Renderer curtainRenderer;

    [Header("Proximity Particle Surge")]
    [Tooltip("Particle system emitting energetic sparks/plasma towards the player when threatened.")]
    [SerializeField] private ParticleSystem proximityParticles;

    [Header("Dynamic Threat Lighting")]
    [Tooltip("Point/Spot light on the wave front casting ominous illumination on the player ship and surroundings.")]
    [SerializeField] private Light threatLight;

    [Tooltip("Base intensity for the threat light.")]
    [SerializeField] private float baseLightIntensity = 2.5f;

    [Tooltip("Extra light intensity boost when player is in proximity of the wave.")]
    [SerializeField] private float maxLightIntensityBoost = 5f;

    [Tooltip("Flicker frequency for the pulsating light.")]
    [SerializeField] private float lightFlickerFrequency = 3.5f;

    private MaterialPropertyBlock mpb;
    private static readonly int Speed1ID = Shader.PropertyToID("_Speed1");
    private static readonly int Speed2ID = Shader.PropertyToID("_Speed2");
    private static readonly int PulseSpeedID = Shader.PropertyToID("_PulseSpeed");
    private static readonly int VoronoiScaleID = Shader.PropertyToID("_VoronoiScale");
    private static readonly int VoronoiPowerID = Shader.PropertyToID("_VoronoiPower");
    private static readonly int EdgeNoiseDistortionID = Shader.PropertyToID("_EdgeNoiseDistortion");

    private float currentDistanceToPlayer = float.MaxValue;
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

        ResolveVisualReferences();
        ApplyConfiguration();
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

        if (config != null)
        {
            config.OnConfigChanged += ApplyConfiguration;
        }

        if (controller != null)
        {
            controller.OnDistanceChanged += HandleDistanceChanged;
            controller.OnThreatStateChanged += HandleThreatStateChanged;
            currentDistanceToPlayer = controller.CurrentDistanceToPlayer;
            currentThreatState = controller.CurrentThreatState;
        }

        ApplyConfiguration();
    }

    private void OnDisable()
    {
        if (config != null)
        {
            config.OnConfigChanged -= ApplyConfiguration;
        }

        if (controller != null)
        {
            controller.OnDistanceChanged -= HandleDistanceChanged;
            controller.OnThreatStateChanged -= HandleThreatStateChanged;
        }
    }

    private void LateUpdate()
    {
        if (!Application.isPlaying) return;

        UpdateVisualCentering();
        UpdateParticleSurge();
        UpdateThreatLight();
    }

    private void HandleDistanceChanged(float distance)
    {
        currentDistanceToPlayer = distance;
    }

    private void HandleThreatStateChanged(WaveThreatState state)
    {
        currentThreatState = state;
    }

    /// <summary>
    /// Centers the visual plane and particle source on the player's X and Y coordinates.
    /// Eliminates visible side edges and prevents flying around the visual boundary in open 3D space.
    /// </summary>
    private void UpdateVisualCentering()
    {
        if (!followPlayerXY || visualRoot == null) return;

        Transform targetPlayer = controller != null ? controller.TargetPlayer : null;
        if (targetPlayer == null) return;

        Vector3 targetPos = new Vector3(targetPlayer.position.x, targetPlayer.position.y, transform.position.z);
        visualRoot.position = targetPos;
    }

    /// <summary>
    /// Modulates proximity particle intensity according to distance to player.
    /// </summary>
    private void UpdateParticleSurge()
    {
        if (proximityParticles == null || config == null) return;

        float targetRate = 0f;
        float maxRate = config.maxParticleEmissionRate;
        float warnDist = controller != null ? controller.WarningDistance : config.warningDistance;

        if (currentThreatState == WaveThreatState.Engulfed)
        {
            targetRate = maxRate * 1.5f;
        }
        else if (currentThreatState == WaveThreatState.Critical || currentThreatState == WaveThreatState.Warning)
        {
            float factor = 1f - Mathf.Clamp01(currentDistanceToPlayer / warnDist);
            targetRate = factor * maxRate;
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

        float warnDist = controller != null ? controller.WarningDistance : (config != null ? config.warningDistance : 100f);
        float proximity = (currentDistanceToPlayer <= warnDist && currentDistanceToPlayer >= 0f)
            ? (1f - (currentDistanceToPlayer / warnDist))
            : (currentThreatState == WaveThreatState.Engulfed ? 1.5f : 0.1f);

        float flicker = Mathf.Sin(Time.time * lightFlickerFrequency) * 0.3f
                      + Mathf.Cos(Time.time * (lightFlickerFrequency * 1.6f)) * 0.15f;

        threatLight.intensity = Mathf.Max(0.5f, baseLightIntensity + (proximity * maxLightIntensityBoost) + (flicker * (1f + proximity)));
    }

    /// <summary>
    /// Synchronizes material shader properties via MaterialPropertyBlock and updates particle box dimensions.
    /// Runs non-destructively in both Editor and Play Mode.
    /// </summary>
    public void ApplyConfiguration()
    {
        if (config == null) return;

        ResolveVisualReferences();

        if (proximityParticles != null)
        {
            var shape = proximityParticles.shape;
            if (shape.shapeType == ParticleSystemShapeType.Box)
            {
                shape.scale = config.sparkEmitterSize;
            }
        }

        if (curtainRenderer != null)
        {
            if (mpb == null) mpb = new MaterialPropertyBlock();
            curtainRenderer.GetPropertyBlock(mpb);

            Vector4 baseSpeed1 = new Vector4(0.006f, 0.012f, 0f, 0f) * config.shaderSpeedMultiplier;
            Vector4 baseSpeed2 = new Vector4(-0.009f, 0.007f, 0f, 0f) * config.shaderSpeedMultiplier;
            mpb.SetVector(Speed1ID, baseSpeed1);
            mpb.SetVector(Speed2ID, baseSpeed2);
            mpb.SetFloat(PulseSpeedID, config.pulseSpeed);
            mpb.SetFloat(VoronoiScaleID, config.filamentWebScale);
            mpb.SetFloat(VoronoiPowerID, config.filamentSharpness);
            mpb.SetFloat(EdgeNoiseDistortionID, config.edgeRaggedness);

            curtainRenderer.SetPropertyBlock(mpb);
        }
    }

    /// <summary>
    /// Auto-resolves visual components from child transforms if unassigned.
    /// </summary>
    private void ResolveVisualReferences()
    {
        if (visualRoot == null)
        {
            var follower = transform.Find("Visual_Follower");
            if (follower != null) visualRoot = follower;
            else
            {
                var child = transform.Find("Visual_Plane");
                if (child != null) visualRoot = child;
            }
        }

        if (proximityParticles == null && visualRoot != null)
        {
            var pChild = visualRoot.Find("Proximity_Particles");
            if (pChild != null) proximityParticles = pChild.GetComponent<ParticleSystem>();
            else proximityParticles = GetComponentInChildren<ParticleSystem>();
        }

        if (curtainRenderer == null && visualRoot != null)
        {
            var curtain = visualRoot.Find("Energy_Curtain");
            if (curtain != null) curtainRenderer = curtain.GetComponent<Renderer>();
        }

        if (threatLight == null)
        {
            threatLight = GetComponentInChildren<Light>();
        }
    }

    private void OnValidate()
    {
        baseLightIntensity = Mathf.Max(0f, baseLightIntensity);
        maxLightIntensityBoost = Mathf.Max(0f, maxLightIntensityBoost);
        lightFlickerFrequency = Mathf.Max(0f, lightFlickerFrequency);
    }
}
