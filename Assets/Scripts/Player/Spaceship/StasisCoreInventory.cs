using System;
using UnityEngine;

/// <summary>
/// Stasis Cores carried by the ship. A core is both a Warp Gate key and a lifeline that can be burned
/// to freeze the Annihilation Wave; a burned core is no longer a key.
/// </summary>
public class StasisCoreInventory : MonoBehaviour
{
    [SerializeField] private StasisCoreConfigSO config;

    public StasisCoreConfigSO Config => config;
    public int Count { get; private set; }
    public int Collected { get; private set; }
    public int RequiredCores => config != null ? config.requiredCores : 2;
    public bool HasRequiredCores => Count >= RequiredCores;

    /// <summary>Raised whenever the number of carried cores changes; the argument is the new count.</summary>
    public event Action<int> OnCountChanged;

    /// <summary>Raised after a core was picked up.</summary>
    public event Action OnCoreCollected;

    private void Awake()
    {
        if (config == null) Debug.LogError("StasisCoreInventory: missing config.", this);
    }

    /// <summary>Adds one core unless the ship already carries the maximum.</summary>
    public bool TryCollect()
    {
        int max = config != null ? config.maxCores : 3;
        if (Count >= max) return false;

        Count++;
        Collected++;
        OnCountChanged?.Invoke(Count);
        OnCoreCollected?.Invoke();
        return true;
    }
}
