using UnityEngine;

[CreateAssetMenu(menuName = "AdAstra/Enemy/Weapon Config", fileName = "EnemyWeaponConfig")]
public class EnemyWeaponConfig : ScriptableObject
{
    [Header("Weapon")]
    [Min(0f)] public float damage = 8f;
    [Min(0.01f)] public float cooldown = 0.6f;
    [Min(0f)] public float range = 12f;

    [Header("Projectile")]
    public EnemyProjectile projectilePrefab;
    [Min(0.01f)] public float projectileSpeed = 25f;
    [Min(0.05f)] public float projectileLifetime = 4f;
    [Min(1)] public int projectilesPerShot = 1;
    [Range(0f, 45f)] public float spreadAngleDegrees = 0f;
    public LayerMask damageLayers = ~0;
}
