using UnityEngine;

/// <summary>
/// Balance settings for hull damage taken from physical impacts (asteroids, station geometry).
/// </summary>
[CreateAssetMenu(fileName = "HullCollisionDamage_Config", menuName = "AdAstra/Player/Hull Collision Damage Config", order = 20)]
public class HullCollisionDamageConfigSO : ScriptableObject
{
    [Tooltip("Impact speed in m/s below which a collision deals no damage (gentle bumps, docking).")]
    [Range(0f, 30f)]
    public float safeImpactSpeed = 5f;

    [Tooltip("Hull damage per m/s of impact speed above the safe threshold.")]
    [Range(0f, 20f)]
    public float damagePerSpeed = 4f;

    [Tooltip("Upper limit of damage from a single impact.")]
    [Range(0f, 500f)]
    public float maxDamage = 150f;

    [Tooltip("Weight of the sliding (tangential) speed. 0 = scrapes are harmless, 1 = scrapes hurt like head-on hits.")]
    [Range(0f, 1f)]
    public float scrapeFactor = 0.2f;

    [Tooltip("How much the obstacle's mass share m / (m + shipMass) scales the damage. 0 = mass ignored, 1 = pebbles barely hurt, boulders and static geometry deal full damage.")]
    [Range(0f, 1f)]
    public float massInfluence = 1f;

    [Tooltip("Minimum time in seconds between two damaging impacts, so one hit with several contact points counts once.")]
    [Range(0f, 2f)]
    public float hitCooldown = 0.3f;

    private void OnValidate()
    {
        safeImpactSpeed = Mathf.Max(0f, safeImpactSpeed);
        damagePerSpeed = Mathf.Max(0f, damagePerSpeed);
        maxDamage = Mathf.Max(0f, maxDamage);
        hitCooldown = Mathf.Max(0f, hitCooldown);
    }
}
