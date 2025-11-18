using UnityEngine;

public abstract class EnemyWeaponBase : MonoBehaviour
{
    [Header("Weapon Settings")]
    public float fireRate = 1f;
    protected float nextFireTime;

    [Header("Projectile Settings")]
    public GameObject projectilePrefab;
    public Transform shootPoint;

    public abstract void TryShoot();
}
