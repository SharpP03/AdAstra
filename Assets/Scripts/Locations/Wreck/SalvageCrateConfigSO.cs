using UnityEngine;

/// <summary>
/// Balance settings for one type of salvage crate: how long it takes to cut and what it gives back.
/// Each crate type (hull, fuel) is its own asset; the crate's material sets its color.
/// </summary>
[CreateAssetMenu(fileName = "SalvageCrate_Config", menuName = "AdAstra/Locations/Salvage Crate Config", order = 51)]
public class SalvageCrateConfigSO : ScriptableObject
{
    [Tooltip("Seconds of beam time needed to cut a crate open. Progress is kept when the beam leaves the crate.")]
    [Range(0.1f, 10f)]
    public float cutTime = 1.5f;

    [Tooltip("Resource a cut crate refills.")]
    public ResourceKind resource = ResourceKind.Health;

    [Tooltip("Fraction of the resource's maximum refilled by one crate.")]
    [Range(0f, 1f)]
    public float rewardFraction = 0.04f;
}
