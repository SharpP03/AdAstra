using System;
using UnityEngine;

/// <summary>
/// ScriptableObject holding balance and tuning parameters for the Warp Gate extraction terminal.
/// Single source of truth for magnetic damping, snap positioning, error penalty timings, and word pool.
/// </summary>
[CreateAssetMenu(fileName = "WarpGate_Config", menuName = "AdAstra/Locations/Warp Gate Config", order = 10)]
public class WarpGateConfigSO : ScriptableObject
{
    [Header("Magnetic Docking & Stabilization")]
    [Tooltip("Rate at which the vessel's linear velocity is damped to zero.")]
    [Range(0.5f, 20f)]
    public float linearDampingRate = 3.5f;

    [Tooltip("Rate at which the vessel's angular velocity is damped to zero.")]
    [Range(0.5f, 20f)]
    public float angularDampingRate = 5f;

    [Tooltip("Rate at which the vessel is pulled toward the DockingSnapPoint position.")]
    [Range(0.5f, 10f)]
    public float snapPositionRate = 2.5f;

    [Tooltip("Rate at which the vessel is rotated to align with the DockingSnapPoint rotation.")]
    [Range(0.5f, 10f)]
    public float snapRotationRate = 3f;

    [Tooltip("Distance threshold (meters) below which the vessel is considered fully stabilized.")]
    [Range(0.05f, 3f)]
    public float snapDistanceTolerance = 0.5f;

    [Header("Terminal Override & Word Pool")]
    [Tooltip("List of thematic system words used for terminal authorization.")]
    public string[] authorizationWordPool = new string[]
    {
        "OVERRIDE",
        "WARP",
        "IGNITE",
        "BYPASS",
        "ESCAPE",
        "EXTRACTION",
        "HYPERDRIVE"
    };

    [Header("Timers & Delays")]
    [Tooltip("Duration of the error lockout penalty in seconds after mistyping a character.")]
    [Range(0.1f, 3f)]
    public float errorLockoutDuration = 0.5f;

    [Tooltip("Buffer delay in seconds after completing the password before triggering victory.")]
    [Range(0.1f, 5f)]
    public float victoryDelayBuffer = 1.0f;

    public event Action OnConfigChanged;

    private void OnValidate()
    {
        linearDampingRate = Mathf.Max(0.1f, linearDampingRate);
        angularDampingRate = Mathf.Max(0.1f, angularDampingRate);
        snapPositionRate = Mathf.Max(0.1f, snapPositionRate);
        snapRotationRate = Mathf.Max(0.1f, snapRotationRate);
        snapDistanceTolerance = Mathf.Max(0.01f, snapDistanceTolerance);
        errorLockoutDuration = Mathf.Max(0.05f, errorLockoutDuration);
        victoryDelayBuffer = Mathf.Max(0.05f, victoryDelayBuffer);

        OnConfigChanged?.Invoke();
    }
}
