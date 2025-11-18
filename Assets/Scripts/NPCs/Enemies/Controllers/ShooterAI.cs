using UnityEngine;

public class ShooterAI : EnemyAIBase
{
    public float turnSpeed = 8f;

    protected override void MoveLogic()
    {
        float dist = Vector3.Distance(transform.position, player.position);

        if (dist > enemyStats.attackRange)
        {
            Vector3 dir = (player.position - transform.position).normalized;
            transform.position += dir * enemyStats.moveSpeed * Time.deltaTime;
        }

        // obrót
        Vector3 lookDir = (player.position - transform.position).normalized;
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            Quaternion.LookRotation(lookDir),
            turnSpeed * Time.deltaTime
        );
    }

    protected override void AttackLogic()
    {
        float dist = Vector3.Distance(transform.position, player.position);

        if (dist <= enemyStats.attackRange)
            weapon?.TryShoot();
    }
}
