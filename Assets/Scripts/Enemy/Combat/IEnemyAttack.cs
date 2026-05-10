using UnityEngine;

public interface IEnemyAttack
{
    float PreferredRange { get; }
    bool CanAttack(Transform target, EnemyContext context, float sqrDistanceToTarget);
    void TryAttack(Transform target, EnemyContext context, float sqrDistanceToTarget);
}
