using System;
using UnityEngine;

/// <summary>
/// Refuels the player gradually while the ship stands nearly still inside the station's trigger zone.
/// The player decides when to leave: fuel flows only as long as the ship stays docked
/// and the station still has fuel left in its limited reserve.
/// </summary>
[RequireComponent(typeof(Collider))]
public class RefuelStation : MonoBehaviour
{
    [SerializeField] private RefuelStationConfigSO config;

    private float remainingFuel;
    private float lastStayTime = float.NegativeInfinity;
    private float lastFlowTime = float.NegativeInfinity;
    private bool isRefueling;

    /// <summary>Raised whenever the station's reserve changes; the argument is the remaining fraction (0–1).</summary>
    public event Action<float> FuelChanged;

    /// <summary>Raised when fuel starts (true) or stops (false) flowing into the ship.</summary>
    public event Action<bool> RefuelingChanged;

    public float RemainingFraction =>
        config != null && config.fuelCapacity > 0f ? remainingFuel / config.fuelCapacity : 0f;

    private void Awake()
    {
        GetComponent<Collider>().isTrigger = true;

        if (config == null)
        {
            Debug.LogError("RefuelStation: missing config.", this);
            enabled = false;
            return;
        }

        remainingFuel = config.fuelCapacity;
    }

    private void FixedUpdate()
    {
        // FixedUpdate runs before this step's trigger callbacks, so the latest possible flow is from the previous
        // step (fixedTime - fixedDeltaTime). Anything older means fuel stopped flowing
        // (ship left, moved, tank full or reserve empty).
        if (isRefueling && lastFlowTime < Time.fixedTime - Time.fixedDeltaTime * 1.5f)
            SetRefueling(false);
    }

    private void OnTriggerStay(Collider other)
    {
        // Unity sends trigger callbacks to disabled behaviours too.
        if (!enabled || remainingFuel <= 0f) return;

        Rigidbody body = other.attachedRigidbody;
        if (body == null || !body.CompareTag("Player")) return;

        // A ship with several colliders reports OnTriggerStay once per collider; refuel once per physics step.
        if (Time.fixedTime == lastStayTime) return;
        if (body.linearVelocity.magnitude > config.maxDockSpeed) return;
        if (!body.TryGetComponent(out FuelRefillable fuel)) return;

        lastStayTime = Time.fixedTime;

        // Only fuel the ship actually accepted leaves the reserve, so a full tank does not drain the station.
        float offered = Mathf.Min(config.fuelPerSecond * Time.fixedDeltaTime, remainingFuel);
        float accepted = fuel.Add(offered);
        if (accepted <= 0f) return;

        lastFlowTime = Time.fixedTime;
        remainingFuel = Mathf.Max(0f, remainingFuel - accepted);
        FuelChanged?.Invoke(RemainingFraction);
        if (!isRefueling) SetRefueling(true);
    }

    private void SetRefueling(bool value)
    {
        isRefueling = value;
        RefuelingChanged?.Invoke(value);
    }
}
