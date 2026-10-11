using UnityEngine;

/// <summary>
/// Refuels the player gradually while the ship stands nearly still inside the station's trigger zone.
/// The player decides when to leave: fuel flows only as long as the ship stays docked.
/// </summary>
[RequireComponent(typeof(Collider))]
public class RefuelStation : MonoBehaviour
{
    [SerializeField] private RefuelStationConfigSO config;

    private float lastRefuelTime = float.NegativeInfinity;

    private void Awake()
    {
        GetComponent<Collider>().isTrigger = true;

        if (config == null)
        {
            Debug.LogError("RefuelStation: missing config.", this);
            enabled = false;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        // Unity sends trigger callbacks to disabled behaviours too.
        if (!enabled) return;

        Rigidbody body = other.attachedRigidbody;
        if (body == null || !body.CompareTag("Player")) return;

        // A ship with several colliders reports OnTriggerStay once per collider; refuel once per physics step.
        if (Time.fixedTime == lastRefuelTime) return;
        if (body.linearVelocity.magnitude > config.maxDockSpeed) return;
        if (!body.TryGetComponent(out FuelRefillable fuel)) return;

        lastRefuelTime = Time.fixedTime;
        fuel.Add(config.fuelPerSecond * Time.fixedDeltaTime);
    }
}
