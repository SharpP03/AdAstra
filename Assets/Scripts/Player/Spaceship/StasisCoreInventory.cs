using System;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Stasis Cores carried by the ship. A core is both a Warp Gate key and a lifeline that can be burned
/// (Stasis action) to freeze the Annihilation Wave; a burned core is no longer a key.
/// A spare core burns on a press; a core still needed as a key requires holding the action.
/// </summary>
public class StasisCoreInventory : MonoBehaviour
{
    [SerializeField] private StasisCoreConfigSO config;

    public StasisCoreConfigSO Config => config;
    public int Count { get; private set; }
    public int Collected { get; private set; }
    public int Burned { get; private set; }
    public int RequiredCores => config != null ? config.requiredCores : 2;
    public bool HasRequiredCores => Count >= RequiredCores;
    public float StasisDuration => config != null ? config.stasisDuration : 6f;
    /// <summary>True once the Warp Gate has taken its key cores; any core left is a spare.</summary>
    public bool KeysDelivered { get; private set; }

    /// <summary>True while the player holds the Stasis action to burn a core that is still a gate key.</summary>
    public bool IsHoldingKeyBurn { get; private set; }
    /// <summary>0–1 progress of the current key burn hold.</summary>
    public float KeyBurnHoldProgress => IsHoldingKeyBurn ? Mathf.Clamp01(holdTimer / KeyBurnHoldTime) : 0f;

    /// <summary>Raised whenever the number of carried cores changes; the argument is the new count.</summary>
    public event Action<int> OnCountChanged;

    /// <summary>Raised after a core was picked up.</summary>
    public event Action OnCoreCollected;

    /// <summary>Raised after a core was burned for Stasis.</summary>
    public event Action OnCoreBurned;

    /// <summary>Raised when a key burn hold starts (true) or ends, by burning or releasing early (false).</summary>
    public event Action<bool> OnKeyBurnHoldChanged;

    private InputAction stasisAction;
    private float holdTimer;

    private float KeyBurnHoldTime => config != null ? config.keyBurnHoldTime : 1f;
    // Burning would leave the ship without enough keys for the gate.
    private bool NextBurnCostsKey => !KeysDelivered && Count <= RequiredCores;

    private void Awake()
    {
        if (config == null) Debug.LogError("StasisCoreInventory: missing config.", this);
    }

    private void Start()
    {
        // Resolved in Start, like the flight actions: PlayerInput sets up and enables its action map on its own.
        stasisAction = GetComponent<PlayerInput>().actions.FindAction("Stasis");
    }

    private void OnDisable()
    {
        SetHoldingKeyBurn(false);
    }

    private void Update()
    {
        // Stasis works in flight, at the station and in the gate terminal (ship controls locked),
        // but not after the run has ended.
        bool canBurn = stasisAction != null && Count > 0 && GameManager.Instance != null && GameManager.Instance.CurrentState == GameState.Playing;
        if (!canBurn)
        {
            SetHoldingKeyBurn(false);
            return;
        }

        if (stasisAction.WasPressedThisFrame())
        {
            if (NextBurnCostsKey) SetHoldingKeyBurn(true);
            else BurnCore();
        }

        if (!IsHoldingKeyBurn) return;

        if (!stasisAction.IsPressed())
        {
            SetHoldingKeyBurn(false);
            return;
        }

        holdTimer += Time.deltaTime;
        if (holdTimer >= KeyBurnHoldTime)
        {
            SetHoldingKeyBurn(false);
            BurnCore();
        }
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

    /// <summary>
    /// Hands the required cores to the Warp Gate. Returns false (and keeps the cores) if the ship has too few.
    /// </summary>
    public bool TryDeliverKeys()
    {
        if (KeysDelivered) return true;
        if (!HasRequiredCores) return false;

        Count -= RequiredCores;
        KeysDelivered = true;
        OnCountChanged?.Invoke(Count);
        return true;
    }

    private void BurnCore()
    {
        Count--;
        Burned++;
        OnCountChanged?.Invoke(Count);
        OnCoreBurned?.Invoke();
    }

    private void SetHoldingKeyBurn(bool holding)
    {
        holdTimer = 0f;
        if (IsHoldingKeyBurn == holding) return;

        IsHoldingKeyBurn = holding;
        OnKeyBurnHoldChanged?.Invoke(holding);
    }
}
