using System;
using UnityEngine;

public enum WaveThreatState
{
    Safe,       // Player is well ahead of the wave (> warningDistance)
    Warning,    // Player is within warning perimeter (<= warningDistance)
    Critical,   // Player is dangerously close (<= criticalDistance)
    Engulfed    // Player is inside / behind the wave boundary (<= 0)
}

/// <summary>
/// Core gameplay controller for the Annihilation Wave hazard:
/// - Kinematic movement along forward axis
/// - Signed distance evaluation to target player
/// - Threat state tracking (Safe -> Warning -> Critical -> Engulfed)
/// - Continuous boundary damage dealing
/// - Event dispatching for HUD and presentation subsystems
/// </summary>
[SelectionBase]
[RequireComponent(typeof(Rigidbody))]
public class AnnihilationWaveController : MonoBehaviour
{
    [Header("Configuration Asset (Single Source of Truth)")]
    [Tooltip("ScriptableObject configuration asset holding all balance and tuning parameters.")]
    [SerializeField] private AnnihilationWaveConfigSO config;

    [Header("Movement & State")]
    [Tooltip("Whether the wave automatically starts moving on Start.")]
    [SerializeField] private bool autoStart = true;

    [Header("Target Tracking & Boundary")]
    [Tooltip("Target transform to track (usually the player's spaceship). If null, resolves from GameManager.")]
    [SerializeField] private Transform targetPlayer;

    [Tooltip("Optional trigger collider used for volume-based contact damage against environmental objects.")]
    [SerializeField] private Collider waveTrigger;

    [Header("Storm Lightning Prefabs (Shared with Sequencer)")]
    [Tooltip("Prefabs of electric storm lightning discharges spawned randomly across the wave front.")]
    [SerializeField] private GameObject[] stormDischargePrefabs;

    // Public properties (Deep Module interface - reading from config as single source of truth)
    public AnnihilationWaveConfigSO Config => config;
    public Transform TargetPlayer => targetPlayer;
    public GameObject[] StormDischargePrefabs => stormDischargePrefabs;
    public float Speed => config != null ? config.speed : 12f;
    public float WarningDistance => config != null ? config.warningDistance : 100f;
    public float CriticalDistance => config != null ? config.criticalDistance : 30f;
    public float DamagePerSecond => config != null ? config.damagePerSecond : 50f;
    public bool IsActive { get; private set; }
    public float CurrentDistanceToPlayer { get; private set; } = float.MaxValue;
    public WaveThreatState CurrentThreatState { get; private set; } = WaveThreatState.Safe;

    // Events for UI/HUD and presentation subsystems
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
    }

    private void Start()
    {
        ResolveTargetPlayer();

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

    /// <summary>
    /// Advances the wave forward using Rigidbody kinematic movement.
    /// </summary>
    private void MoveWave()
    {
        Vector3 delta = transform.forward * (Speed * Time.fixedDeltaTime);
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
        else if (distance <= CriticalDistance)
        {
            newState = WaveThreatState.Critical;
        }
        else if (distance <= WarningDistance)
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
    /// Applies continuous damage to the tracked player whenever they are engulfed behind the wave boundary (signed distance <= 0).
    /// Mathematical plane evaluation guarantees 100% collision reliability independent of physics timestep or velocity tunneling.
    /// </summary>
    private void HandlePlaneBoundaryDamage()
    {
        if (CurrentThreatState != WaveThreatState.Engulfed) return;

        if (cachedPlayerDamageable != null)
        {
            cachedPlayerDamageable.TakeDamage(DamagePerSecond * Time.fixedDeltaTime);
        }
    }

    /// <summary>
    /// Optional volume trigger damage for non-player environmental IDamageable objects (e.g. drifting obstacles).
    /// Player damage is handled exclusively via signed distance in HandlePlaneBoundaryDamage.
    /// </summary>
    private void OnTriggerStay(Collider other)
    {
        if (targetPlayer != null && other.transform.root == targetPlayer.root)
        {
            return;
        }

        if (other.TryGetComponent<IDamageable>(out var damageable))
        {
            damageable.TakeDamage(DamagePerSecond * Time.fixedDeltaTime);
        }
        else
        {
            var parentDamageable = other.GetComponentInParent<IDamageable>();
            parentDamageable?.TakeDamage(DamagePerSecond * Time.fixedDeltaTime);
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
        if (config != null) config.speed = Mathf.Max(0f, newSpeed);
    }

    public void SetActive(bool active)
    {
        IsActive = active;
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
        Vector3 critCenter = center + forward * CriticalDistance;
        Gizmos.DrawLine(critCenter - right, critCenter + right);

        // Draw warning distance plane
        Gizmos.color = new Color(1f, 0.92f, 0.016f, 0.5f);
        Vector3 warnCenter = center + forward * WarningDistance;
        Gizmos.DrawLine(warnCenter - right, warnCenter + right);

        // Forward vector
        Gizmos.color = Color.cyan;
        Gizmos.DrawRay(center, forward * 20f);
    }
}
