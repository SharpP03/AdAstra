using System;
using UnityEngine;

public class HealthSystem : MonoBehaviour, IDamageable
{
    [SerializeField]
    private float maxHealth = 300f;
    public float MaxHealth => maxHealth; //readonly ref
    public float currentHealth { get; private set; }

    public event Action<float, float> OnHealthChanged;
    public event Action OnDied;

    private bool isDead = false;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float damageValue)
    {
        if (isDead) return;

        currentHealth = Mathf.Max(0f, currentHealth - damageValue);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        if (currentHealth <= 0f)
        {
            isDead = true;
            OnDied?.Invoke();
        }
    }

    public void AddHealth(float amount)
    {
        if (isDead) return;

        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }
}
