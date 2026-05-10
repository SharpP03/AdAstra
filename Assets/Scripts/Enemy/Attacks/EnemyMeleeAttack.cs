using UnityEngine;

[RequireComponent(typeof(EnemyContext))]
[DisallowMultipleComponent]
public class EnemyMeleeAttack : MonoBehaviour, IEnemyAttack
{
    [SerializeField] private EnemyContext enemyContext;
    [SerializeField] private bool useEnemyConfigValues = true;
    [SerializeField] [Min(0f)] private float attackRangeOverride = 2.2f;
    [SerializeField] [Min(0f)] private float attackDamageOverride = 10f;
    [SerializeField] [Min(0.01f)] private float attackCooldownOverride = 1f;

    private float nextAttackTime;
    private Transform cachedTarget;
    private IDamageable cachedDamageable;

    public float PreferredRange => ResolveAttackRange();

    private void Reset()
    {
        enemyContext = GetComponent<EnemyContext>();
    }

    private void Awake()
    {
        if (enemyContext == null) enemyContext = GetComponent<EnemyContext>();
    }

    public bool CanAttack(Transform target, EnemyContext context, float sqrDistanceToTarget)
    {
        if (target == null) return false;
        if (Time.time < nextAttackTime) return false;

        var attackRange = ResolveAttackRange();
        if (sqrDistanceToTarget > attackRange * attackRange) return false;

        return ResolveDamageable(target) != null;
    }

    public void TryAttack(Transform target, EnemyContext context, float sqrDistanceToTarget)
    {
        if (!CanAttack(target, context, sqrDistanceToTarget)) return;

        var damageable = ResolveDamageable(target);
        if (damageable == null) return;

        damageable.TakeDamage(ResolveAttackDamage());
        nextAttackTime = Time.time + ResolveAttackCooldown();
    }

    private float ResolveAttackRange()
    {
        if (useEnemyConfigValues && enemyContext != null && enemyContext.Config != null)
            return Mathf.Max(0f, enemyContext.Config.attackRange);

        return Mathf.Max(0f, attackRangeOverride);
    }

    private float ResolveAttackDamage()
    {
        if (useEnemyConfigValues && enemyContext != null && enemyContext.Config != null)
            return Mathf.Max(0f, enemyContext.Config.attackDamage);

        return Mathf.Max(0f, attackDamageOverride);
    }

    private float ResolveAttackCooldown()
    {
        if (useEnemyConfigValues && enemyContext != null && enemyContext.Config != null)
            return Mathf.Max(0.01f, enemyContext.Config.attackCooldown);

        return Mathf.Max(0.01f, attackCooldownOverride);
    }

    private IDamageable ResolveDamageable(Transform target)
    {
        if (target == cachedTarget && cachedDamageable != null) return cachedDamageable;

        cachedTarget = target;
        cachedDamageable = EnemyDamageableResolver.Resolve(target.gameObject);
        return cachedDamageable;
    }
}
