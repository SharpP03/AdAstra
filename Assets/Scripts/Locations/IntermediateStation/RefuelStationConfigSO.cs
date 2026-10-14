using UnityEngine;

/// <summary>
/// Balance settings for the intermediate refuel station.
/// </summary>
[CreateAssetMenu(fileName = "IntermediateStation_Config", menuName = "AdAstra/Locations/Refuel Station Config", order = 30)]
public class RefuelStationConfigSO : ScriptableObject
{
    [Tooltip("Fuel added per second while the ship stands still inside the station zone.")]
    [Range(0f, 50f)]
    public float fuelPerSecond = 8f;

    [Tooltip("Ship speed in m/s below which it counts as docked and receives fuel.")]
    [Range(0.1f, 10f)]
    public float maxDockSpeed = 2f;

    [Tooltip("Braking (1/s) applied to a ship with an empty tank inside the zone, so a drifting ship gets caught. Higher = stops in a shorter distance (roughly speed / catchDrag meters).")]
    [Range(0f, 20f)]
    public float catchDrag = 5f;

    [Tooltip("Total fuel the station can hand out during one run. Once empty, it stops refueling.")]
    [Range(0f, 1000f)]
    public float fuelCapacity = 150f;

    private void OnValidate()
    {
        fuelPerSecond = Mathf.Max(0f, fuelPerSecond);
        maxDockSpeed = Mathf.Max(0.1f, maxDockSpeed);
        fuelCapacity = Mathf.Max(0f, fuelCapacity);
        catchDrag = Mathf.Max(0f, catchDrag);
    }
}
