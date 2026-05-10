using System;
using UnityEngine;

[RequireComponent(typeof(EnemyContext))]
public class EnemyHealth : MonoBehaviour, IDamageable
{
    [SerializeField] private EnemyContext enemyContext;
    [SerializeField] [Min(0f)] private float maxHealthOverride = 0f;
    [SerializeField] private bool destroyOnDeath = true;

    public float MaxHealth { get; private set; }
    public float CurrentHealth { get; private set; }

    public event Action<EnemyHealth> Died;

    private bool isDead;

    private void Reset()
    {
        enemyContext = GetComponent<EnemyContext>();
    }

    private void Awake()
    {
        if (enemyContext == null)
            enemyContext = GetComponent<EnemyContext>();

        var fallbackHealth = 30f;
        if (enemyContext != null && enemyContext.Config != null)
            fallbackHealth = enemyContext.Config.maxHealth;
        else
            Debug.LogWarning(
                $"EnemyHealth on '{name}' could not read maxHealth from EnemyConfig. Using fallback value: {fallbackHealth}.");

        MaxHealth = maxHealthOverride > 0f ? maxHealthOverride : fallbackHealth;
        CurrentHealth = MaxHealth;
    }

    public void TakeDamage(float amount)
    {
        if (isDead || amount <= 0f) return;

        CurrentHealth = Mathf.Max(0f, CurrentHealth - amount);
        if (CurrentHealth <= 0f)
            Die();
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;

        Died?.Invoke(this);

        if (destroyOnDeath)
            Destroy(gameObject);
    }
}
