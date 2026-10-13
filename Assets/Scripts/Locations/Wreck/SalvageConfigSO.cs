using UnityEngine;

/// <summary>
/// Balance settings for wreck salvage: the reach and aim assist of the ship's salvage laser,
/// how long a cargo crate takes to cut and what it gives back.
/// </summary>
[CreateAssetMenu(fileName = "Salvage_Config", menuName = "AdAstra/Locations/Salvage Config", order = 50)]
public class SalvageConfigSO : ScriptableObject
{
    [Header("Laser")]
    [Tooltip("Length of the aim-assist box in front of the nose: the furthest a crate can be cut from.")]
    [Range(5f, 150f)]
    public float laserRange = 40f;

    [Tooltip("Width and height of the aim-assist box. A crate anywhere inside it pulls the beam onto itself.")]
    [Range(0.5f, 30f)]
    public float assistBoxWidth = 6f;

    [Tooltip("Length of the beam fired without a target. It has no effect; it only shows the laser works.")]
    [Range(0f, 30f)]
    public float idleBeamLength = 6f;

    [Tooltip("Beam width without a target, relative to the beam cutting a crate.")]
    [Range(0.05f, 1f)]
    public float idleBeamWidthScale = 0.4f;

    [Header("Crates")]
    [Tooltip("Seconds of beam time needed to cut a crate open. Progress is kept when the beam leaves the crate.")]
    [Range(0.1f, 10f)]
    public float cutTime = 1.5f;

    [Tooltip("Resource a cut crate refills.")]
    public ResourceKind crateResource = ResourceKind.Health;

    [Tooltip("Fraction of the resource's maximum refilled by one crate.")]
    [Range(0f, 1f)]
    public float rewardFraction = 0.04f;
}
