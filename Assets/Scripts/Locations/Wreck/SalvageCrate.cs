using System;
using UnityEngine;

/// <summary>
/// Cargo crate drifting around a wreck. The ship's salvage laser cuts it; cutting progress adds up over
/// separate passes of the beam. Once cut through, the crate bursts and refills the ship's resource.
/// </summary>
[SelectionBase]
public class SalvageCrate : MonoBehaviour
{
    [SerializeField] private SalvageCrateConfigSO config;

    /// <summary>Cutting progress from 0 to 1. It is kept when the beam leaves the crate.</summary>
    public float CutProgress { get; private set; }
    public bool IsOpened { get; private set; }
    /// <summary>World point where the beam touched the crate most recently.</summary>
    public Vector3 ContactPoint { get; private set; }

    /// <summary>Raised every frame the beam cuts the crate; the argument is the new progress.</summary>
    public event Action<float> OnCutProgress;

    /// <summary>Raised once when the crate is cut through; the argument is the amount actually refilled.</summary>
    public event Action<float> OnOpened;

    private void Awake()
    {
        if (config == null) Debug.LogError("SalvageCrate: missing config.", this);
    }

    /// <summary>
    /// Applies <paramref name="deltaTime"/> seconds of beam time. <paramref name="cutter"/> is the ship
    /// whose resource the crate refills when it opens.
    /// </summary>
    public void Cut(float deltaTime, GameObject cutter, Vector3 contactPoint)
    {
        if (IsOpened || config == null) return;

        ContactPoint = contactPoint;
        CutProgress = Mathf.Clamp01(CutProgress + deltaTime / config.cutTime);
        OnCutProgress?.Invoke(CutProgress);

        if (CutProgress >= 1f) Open(cutter);
    }

    private void Open(GameObject cutter)
    {
        IsOpened = true;

        float refilled = 0f;
        foreach (IRefillable refillable in cutter.GetComponents<IRefillable>())
        {
            if (refillable.Kind == config.resource)
                refilled += refillable.Add(refillable.Max * config.rewardFraction);
        }

        OnOpened?.Invoke(refilled);
        gameObject.SetActive(false);
    }
}
