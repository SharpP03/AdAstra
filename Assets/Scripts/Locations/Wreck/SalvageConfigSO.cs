using UnityEngine;

/// <summary>
/// Balance settings for the ship's salvage laser: its reach and aim assist.
/// Crate settings (cut time, reward) live in <see cref="SalvageCrateConfigSO"/>, one asset per crate type.
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
}
