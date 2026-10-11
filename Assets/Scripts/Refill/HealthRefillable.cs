using UnityEngine;

[RequireComponent(typeof(HealthSystem))]
public class HealthRefillable : MonoBehaviour, IRefillable
{
    private HealthSystem healthSystem;

    public ResourceKind Kind => ResourceKind.Health;
    public float Max => healthSystem != null ? healthSystem.MaxHealth : 0f;

    private void Awake()
    {
        healthSystem = GetComponent<HealthSystem>();
    }

    public float Add(float amount)
    {
        if (healthSystem == null) return 0f;

        float before = healthSystem.currentHealth;
        healthSystem.AddHealth(amount);
        return healthSystem.currentHealth - before;
    }
}
