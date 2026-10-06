using System;
using UnityEngine;

/// <summary>
/// Controller for the Warp Gate extraction destination:
/// - Detects player vessel entry into the magnetic docking perimeter
/// - Locks vessel flight controls and zeroes input buffers
/// - Smoothly dampens linear & angular velocities in FixedUpdate
/// - Magnetically draws and stabilizes vessel at DockingSnapPoint
/// </summary>
[SelectionBase]
public class WarpGateController : MonoBehaviour
{
    [Header("Configuration")]
    [Tooltip("ScriptableObject configuration asset holding damping and terminal parameters.")]
    [SerializeField] private WarpGateConfigSO config;

    [Header("Docking Anchor")]
    [Tooltip("Target transform where the ship should be snapped and stabilized during extraction.")]
    [SerializeField] private Transform dockingSnapPoint;

    [Tooltip("Optional trigger collider defining the magnetic docking capture zone.")]
    [SerializeField] private Collider dockingTrigger;

    // Public properties
    public WarpGateConfigSO Config => config;
    public Transform DockingSnapPoint => dockingSnapPoint;
    public Player_Spaceship DockedPlayer { get; private set; }
    public bool IsPlayerDocked => DockedPlayer != null;
    public bool IsStabilized { get; private set; }

    // Events
    public event Action<Player_Spaceship> OnPlayerDocked;
    public event Action OnPlayerStabilized;

    private Rigidbody dockedRb;

    private void Awake()
    {
        if (dockingSnapPoint == null)
        {
            // Default to this transform if snap point not explicitly assigned
            dockingSnapPoint = transform;
        }

        if (dockingTrigger == null)
        {
            dockingTrigger = GetComponent<Collider>();
        }

        if (dockingTrigger != null)
        {
            dockingTrigger.isTrigger = true;
        }
    }

    private void FixedUpdate()
    {
        if (IsPlayerDocked && dockedRb != null)
        {
            ApplyMagneticDockingStabilization();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (IsPlayerDocked) return;

        Player_Spaceship player = other.GetComponentInParent<Player_Spaceship>();
        if (player != null)
        {
            DockPlayer(player);
        }
    }

    public void DockPlayer(Player_Spaceship player)
    {
        if (player == null || IsPlayerDocked) return;

        DockedPlayer = player;
        dockedRb = player.GetComponent<Rigidbody>();
        IsStabilized = false;

        // Isolate flight controls immediately
        player.SetFlightControlsLocked(true);

        OnPlayerDocked?.Invoke(player);
    }

    private void ApplyMagneticDockingStabilization()
    {
        float linearDamping = config != null ? config.linearDampingRate : 3.5f;
        float angularDamping = config != null ? config.angularDampingRate : 5f;
        float snapPosRate = config != null ? config.snapPositionRate : 2.5f;
        float snapRotRate = config != null ? config.snapRotationRate : 3f;
        float snapTolerance = config != null ? config.snapDistanceTolerance : 0.5f;

        // 1. Damp linear & angular velocity to zero
        dockedRb.linearVelocity = Vector3.Lerp(dockedRb.linearVelocity, Vector3.zero, linearDamping * Time.fixedDeltaTime);
        dockedRb.angularVelocity = Vector3.Lerp(dockedRb.angularVelocity, Vector3.zero, angularDamping * Time.fixedDeltaTime);

        // 2. Smoothly pull towards docking snap point
        Vector3 targetPos = dockingSnapPoint != null ? dockingSnapPoint.position : transform.position;
        Quaternion targetRot = dockingSnapPoint != null ? dockingSnapPoint.rotation : transform.rotation;

        Vector3 nextPos = Vector3.Lerp(dockedRb.position, targetPos, snapPosRate * Time.fixedDeltaTime);
        Quaternion nextRot = Quaternion.Slerp(dockedRb.rotation, targetRot, snapRotRate * Time.fixedDeltaTime);

        dockedRb.MovePosition(nextPos);
        dockedRb.MoveRotation(nextRot);

        // 3. Check stabilization threshold
        float distToSnap = Vector3.Distance(dockedRb.position, targetPos);
        if (!IsStabilized && distToSnap <= snapTolerance)
        {
            IsStabilized = true;
            OnPlayerStabilized?.Invoke();
        }
    }

    private void OnDrawGizmosSelected()
    {
        Transform snap = dockingSnapPoint != null ? dockingSnapPoint : transform;
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(snap.position, 1.5f);
        Gizmos.DrawRay(snap.position, snap.forward * 4f);
    }
}
