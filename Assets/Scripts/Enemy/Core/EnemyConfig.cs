using UnityEngine;

[CreateAssetMenu(menuName = "AdAstra/Enemy/Enemy Config", fileName = "EnemyConfig")]
public class EnemyConfig : ScriptableObject
{
    [Header("Stats")]
    [Min(1f)] public float maxHealth = 30f;

    [Header("Detection")]
    [Min(0f)] public float detectionRange = 25f;
    [Min(0f)] public float attackRange = 2.2f;

    [Header("Movement")]
    [Min(0f)] public float moveSpeed = 8f;
    [Min(0f)] public float acceleration = 35f;
    [Min(0f)] public float deceleration = 50f;
    [Min(0f)] public float turnSpeed = 8f;
    [Min(0f)] public float stoppingDistance = 1.6f;
    [Min(0f)] public float engagementRangeTolerance = 0.5f;

    [Header("Attack")]
    [Min(0f)] public float attackDamage = 10f;
    [Min(0.01f)] public float attackCooldown = 1f;
}
