using UnityEngine;

/// <summary>
/// Balance settings for the player ship's flight model and fuel burn.
/// Kept in one asset so boost speed and boost fuel cost are tuned side by side.
/// </summary>
[CreateAssetMenu(fileName = "ShipFlight_Config", menuName = "AdAstra/Player/Ship Flight Config", order = 10)]
public class ShipFlightConfigSO : ScriptableObject
{
    [Header("Thrust")]
    [Tooltip("Thrust acceleration in m/s^2. With linear drag it sets the top cruise speed (roughly thrust / drag).")]
    public float thrustAcceleration = 23f;

    [Tooltip("Thrust multiplier while boosting.")]
    public float boostMultiplier = 2f;

    [Tooltip("How fast thrust ramps toward its target (1/s). Higher = snappier throttle.")]
    public float throttleResponse = 4f;

    [Header("Drag")]
    [Tooltip("Rigidbody linear damping. Lower = the ship coasts longer after releasing thrust.")]
    public float linearDrag = 1f;

    [Tooltip("Rigidbody angular damping. Lower = rotation keeps spinning longer.")]
    public float angularDrag = 1f;

    [Header("Rotation")]
    [Tooltip("Mouse input multiplier for pitch and yaw.")]
    public float mouseSensitivity = 0.1f;

    [Tooltip("Pitch and yaw torque per unit of mouse input.")]
    public float pitchYawTorque = 0.5f;

    [Tooltip("Roll torque.")]
    public float rollTorque = 1f;

    [Header("Fuel")]
    public float maxFuel = 280f;

    [Tooltip("Fuel burned per second while thrusting.")]
    public float fuelBurnRate = 2.2f;

    [Tooltip("Boost burn as a multiple of fuelBurnRate. Keep it above boostMultiplier so boosting costs more per meter.")]
    public float boostFuelMultiplier = 3.2f;

    private void OnValidate()
    {
        thrustAcceleration = Mathf.Max(0f, thrustAcceleration);
        boostMultiplier = Mathf.Max(1f, boostMultiplier);
        throttleResponse = Mathf.Max(0f, throttleResponse);
        linearDrag = Mathf.Max(0f, linearDrag);
        angularDrag = Mathf.Max(0f, angularDrag);
        maxFuel = Mathf.Max(1f, maxFuel);
        fuelBurnRate = Mathf.Max(0f, fuelBurnRate);
        boostFuelMultiplier = Mathf.Max(1f, boostFuelMultiplier);
    }
}
