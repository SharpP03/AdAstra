using UnityEngine;

/// <summary>
/// Balance settings for Stasis Cores: how many open the Warp Gate,
/// how much hull a pickup repairs and how long a burned core freezes the Annihilation Wave.
/// </summary>
[CreateAssetMenu(fileName = "StasisCore_Config", menuName = "AdAstra/Locations/Stasis Core Config", order = 40)]
public class StasisCoreConfigSO : ScriptableObject
{
    [Tooltip("Cores the Warp Gate takes as the key. Lower it to 1 if the dilemma turns out too harsh.")]
    [Range(1, 3)]
    public int requiredCores = 2;

    [Tooltip("Fraction of the maximum hull repaired instantly when a core is collected.")]
    [Range(0f, 1f)]
    public float hullRepairFraction = 0.2f;

    [Tooltip("Seconds the Annihilation Wave stays frozen after a core is burned.")]
    [Range(1f, 20f)]
    public float stasisDuration = 6f;

    [Tooltip("Seconds the Stasis key must be held to burn a core that is still needed as a gate key.")]
    [Range(0.1f, 3f)]
    public float keyBurnHoldTime = 1f;

    private void OnValidate()
    {
        requiredCores = Mathf.Max(1, requiredCores);
    }
}
