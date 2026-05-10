using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(EnemyContext))]
[RequireComponent(typeof(EnemyGun))]
public class EnemyRangedAttack : MonoBehaviour, IEnemyAttack
{
    [SerializeField] private EnemyContext enemyContext;
    [SerializeField] private EnemyGun enemyGun;
    [SerializeField] private bool preferredRangeFromWeapon = true;
    [SerializeField] [Min(0f)] private float preferredRangeOverride = 10f;

    public float PreferredRange => preferredRangeFromWeapon ? ResolveWeaponRange() : preferredRangeOverride;

    private void Reset()
    {
        enemyContext = GetComponent<EnemyContext>();
        enemyGun = GetComponent<EnemyGun>();
    }

    private void Awake()
    {
        if (enemyContext == null) enemyContext = GetComponent<EnemyContext>();
        if (enemyGun == null) enemyGun = GetComponent<EnemyGun>();
    }

    public bool CanAttack(Transform target, EnemyContext context, float sqrDistanceToTarget)
    {
        if (target == null || enemyGun == null || !enemyGun.CanFire) return false;

        var attackRange = ResolveWeaponRange();
        if (attackRange <= 0f) return false;

        return sqrDistanceToTarget <= attackRange * attackRange;
    }

    public void TryAttack(Transform target, EnemyContext context, float sqrDistanceToTarget)
    {
        if (!CanAttack(target, context, sqrDistanceToTarget)) return;

        var fireOrigin = enemyGun.FireOriginPosition;
        var aimDirection = target.position - fireOrigin;
        if (aimDirection.sqrMagnitude <= 0.0001f)
            aimDirection = transform.forward;

        enemyGun.TryFire(aimDirection.normalized, gameObject);
    }

    private float ResolveWeaponRange()
    {
        return enemyGun != null ? enemyGun.EffectiveRange : 0f;
    }
}
