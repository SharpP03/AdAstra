using UnityEngine;

[RequireComponent(typeof(EnemyContext))]
public class EnemyMovement : MonoBehaviour
{
    [SerializeField] private EnemyContext enemyContext;

    private void Reset()
    {
        enemyContext = GetComponent<EnemyContext>();
    }

    private void Awake()
    {
        if (enemyContext == null) enemyContext = GetComponent<EnemyContext>();
    }

    private void FixedUpdate()
    {
        if (enemyContext == null || enemyContext.Config == null) return;

        var target = enemyContext.TargetProvider.CurrentTarget;
        var rb = enemyContext.EnemyRigidbody;
        var config = enemyContext.Config;

        if (target == null)
        {
            SlowDown(rb, config);
            return;
        }

        var toTarget = target.position - rb.position;
        var distance = toTarget.magnitude;
        if (distance > config.detectionRange)
        {
            SlowDown(rb, config);
            return;
        }

        var preferredRange = ResolvePreferredRange(config);
        var tolerance = Mathf.Max(0f, config.engagementRangeTolerance);
        var minRange = Mathf.Max(0f, preferredRange - tolerance);
        var maxRange = preferredRange + tolerance;

        if (distance > maxRange)
            MoveInDirection(rb, config, toTarget.normalized);
        else if (distance < minRange)
            MoveInDirection(rb, config, -toTarget.normalized);
        else
            SlowDown(rb, config);

        if (toTarget.sqrMagnitude > 0.0001f)
            RotateTowardsTarget(rb, config, toTarget.normalized);
    }

    private float ResolvePreferredRange(EnemyConfig config)
    {
        var preferredRange = config.stoppingDistance;
        var attackController = enemyContext.AttackController;
        if (attackController != null && attackController.HasAttackModule)
            preferredRange = attackController.PreferredRange;

        return Mathf.Max(0f, preferredRange);
    }

    private static void MoveInDirection(Rigidbody rb, EnemyConfig config, Vector3 direction)
    {
        if (direction.sqrMagnitude <= 0.0001f)
        {
            SlowDown(rb, config);
            return;
        }

        var desiredVelocity = direction.normalized * config.moveSpeed;
        rb.linearVelocity = Vector3.MoveTowards(
            rb.linearVelocity,
            desiredVelocity,
            config.acceleration * Time.fixedDeltaTime
        );
    }

    private static void RotateTowardsTarget(Rigidbody rb, EnemyConfig config, Vector3 lookDirection)
    {
        var desiredRotation = Quaternion.LookRotation(lookDirection, Vector3.up);
        rb.MoveRotation(Quaternion.Slerp(rb.rotation, desiredRotation, config.turnSpeed * Time.fixedDeltaTime));
    }

    private static void SlowDown(Rigidbody rb, EnemyConfig config)
    {
        rb.linearVelocity = Vector3.MoveTowards(
            rb.linearVelocity,
            Vector3.zero,
            config.deceleration * Time.fixedDeltaTime
        );
    }
}
