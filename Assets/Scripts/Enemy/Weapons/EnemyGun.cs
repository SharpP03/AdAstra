using UnityEngine;

[DisallowMultipleComponent]
public class EnemyGun : MonoBehaviour
{
    [SerializeField] private EnemyWeaponConfig weaponConfig;
    [SerializeField] private EnemyProjectilePool projectilePool;
    [SerializeField] private Transform muzzlePoint;
    [SerializeField] private bool autoFindPoolInParents = true;

    private float nextFireTime;

    public EnemyWeaponConfig WeaponConfig => weaponConfig;
    public float EffectiveRange => weaponConfig != null ? Mathf.Max(0f, weaponConfig.range) : 0f;
    public bool CanFire => weaponConfig != null
                           && weaponConfig.projectilePrefab != null
                           && projectilePool != null
                           && Time.time >= nextFireTime;
    public Vector3 FireOriginPosition => muzzlePoint != null ? muzzlePoint.position : transform.position;

    private void Reset()
    {
        if (muzzlePoint == null)
            muzzlePoint = transform;
        if (projectilePool == null)
            projectilePool = GetComponentInParent<EnemyProjectilePool>();
    }

    private void Awake()
    {
        if (muzzlePoint == null)
            muzzlePoint = transform;

        if (projectilePool == null && autoFindPoolInParents)
            projectilePool = GetComponentInParent<EnemyProjectilePool>();

        if (weaponConfig == null)
            Debug.LogWarning($"EnemyGun on '{name}' has no EnemyWeaponConfig assigned.");
        else if (weaponConfig.projectilePrefab == null)
            Debug.LogWarning($"EnemyGun on '{name}' has WeaponConfig '{weaponConfig.name}' without projectilePrefab.");

        if (projectilePool == null)
            Debug.LogWarning($"EnemyGun on '{name}' has no EnemyProjectilePool assigned.");
    }

    public void TryFire(Vector3 aimDirection, GameObject ownerRoot)
    {
        if (!CanFire) return;

        var safeDirection = aimDirection.sqrMagnitude > 0.0001f ? aimDirection.normalized : transform.forward;
        var projectileCount = Mathf.Max(1, weaponConfig.projectilesPerShot);

        for (var i = 0; i < projectileCount; i++)
            SpawnProjectile(safeDirection, ownerRoot);

        nextFireTime = Time.time + weaponConfig.cooldown;
    }

    private void SpawnProjectile(Vector3 aimDirection, GameObject ownerRoot)
    {
        var projectile = projectilePool.Get(weaponConfig.projectilePrefab);
        if (projectile == null)
        {
            Debug.LogWarning($"EnemyGun on '{name}' could not fetch projectile from pool.");
            return;
        }

        var spreadDirection = ApplySpread(aimDirection, weaponConfig.spreadAngleDegrees);
        projectile.Launch(FireOriginPosition, spreadDirection, weaponConfig, ownerRoot);
    }

    private static Vector3 ApplySpread(Vector3 baseDirection, float spreadAngleDegrees)
    {
        if (spreadAngleDegrees <= 0f)
            return baseDirection.normalized;

        var spreadAxis = Random.onUnitSphere;
        var spread = Random.Range(-spreadAngleDegrees, spreadAngleDegrees);
        return (Quaternion.AngleAxis(spread, spreadAxis) * baseDirection).normalized;
    }
}
