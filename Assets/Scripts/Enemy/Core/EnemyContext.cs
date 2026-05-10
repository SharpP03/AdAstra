using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(EnemyTargetProvider))]
public class EnemyContext : MonoBehaviour
{
    [SerializeField] private EnemyConfig config;
    [SerializeField] private Rigidbody enemyRigidbody;
    [SerializeField] private EnemyTargetProvider targetProvider;
    [SerializeField] private EnemyAttackController attackController;

    // readonly
    public EnemyConfig Config => config;
    public Rigidbody EnemyRigidbody => enemyRigidbody;
    public EnemyTargetProvider TargetProvider => targetProvider;
    public EnemyAttackController AttackController => attackController;

    private void Reset()
    {
        enemyRigidbody = GetComponent<Rigidbody>();
        targetProvider = GetComponent<EnemyTargetProvider>();
        attackController = GetComponent<EnemyAttackController>();
    }

    private void Awake()
    {
        if (enemyRigidbody == null) enemyRigidbody = GetComponent<Rigidbody>();
        if (targetProvider == null) targetProvider = GetComponent<EnemyTargetProvider>();
        if (attackController == null) attackController = GetComponent<EnemyAttackController>();

        if (config == null)
            Debug.LogWarning($"EnemyContext on '{name}' has no EnemyConfig assigned.");
    }
}
