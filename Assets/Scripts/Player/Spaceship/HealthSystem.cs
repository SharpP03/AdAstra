using System;
using UnityEngine;

public class HealthSystem : MonoBehaviour, IDamageable
{

    #if UNITY_EDITOR
    [ContextMenu("Debug: take 60 damage")]
    private void DebugTakeDamage() => TakeDamage(60f);
    #endif
    
    [SerializeField]
    private float maxHealth = 300f;
    public float MaxHealth => maxHealth; //readonly ref
    public float currentHealth { get; private set; }

    public event Action<float, float> OnHealthChanged;
    public event Action OnDied;
    /// <summary>Raised when the hull is repaired; the argument is the amount actually restored.</summary>
    public event Action<float> OnRepaired;

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

        float before = currentHealth;
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        if (currentHealth > before) OnRepaired?.Invoke(currentHealth - before);
    }
}
