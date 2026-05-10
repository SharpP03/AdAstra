    using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(EnemyContext))]
public class EnemyAttackController : MonoBehaviour
{
    [SerializeField] private EnemyContext enemyContext;
    [SerializeField] private MonoBehaviour attackModuleBehaviour;

    private IEnemyAttack attackModule;

    public bool HasAttackModule => attackModule != null;
    public float PreferredRange => attackModule != null ? Mathf.Max(0f, attackModule.PreferredRange) : ResolveFallbackRange();

    private void Reset()
    {
        enemyContext = GetComponent<EnemyContext>();
        AutoAssignAttackModule();
    }

    private void Awake()
    {
        if (enemyContext == null)
            enemyContext = GetComponent<EnemyContext>();

        ResolveAttackModule();

        if (attackModule == null)
            Debug.LogWarning($"EnemyAttackController on '{name}' has no valid IEnemyAttack module assigned.");
    }

    private void Update()
    {
        if (enemyContext == null || enemyContext.Config == null || attackModule == null) return;

        var target = enemyContext.TargetProvider.CurrentTarget;
        if (target == null) return;

        var sqrDistance = (target.position - transform.position).sqrMagnitude;
        var sqrDetectionRange = enemyContext.Config.detectionRange * enemyContext.Config.detectionRange;
        if (sqrDistance > sqrDetectionRange) return;

        if (!attackModule.CanAttack(target, enemyContext, sqrDistance)) return;
        attackModule.TryAttack(target, enemyContext, sqrDistance);
    }

    private float ResolveFallbackRange()
    {
        if (enemyContext != null && enemyContext.Config != null)
            return Mathf.Max(0f, enemyContext.Config.stoppingDistance);

        return 0f;
    }

    private void ResolveAttackModule()
    {
        if (attackModuleBehaviour == null)
            AutoAssignAttackModule();

        attackModule = attackModuleBehaviour as IEnemyAttack;
        if (attackModuleBehaviour != null && attackModule == null)
            Debug.LogWarning(
                $"EnemyAttackController on '{name}' has attack module '{attackModuleBehaviour.GetType().Name}' that does not implement IEnemyAttack.");
    }

    private void AutoAssignAttackModule()
    {
        var behaviours = GetComponents<MonoBehaviour>();
        for (var i = 0; i < behaviours.Length; i++)
        {
            if (behaviours[i] is IEnemyAttack)
            {
                attackModuleBehaviour = behaviours[i];
                return;
            }
        }

        attackModuleBehaviour = null;
    }
}
