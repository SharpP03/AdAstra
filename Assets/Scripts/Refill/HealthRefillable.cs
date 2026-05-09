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

    public void Add(float amount)
    {
        if (healthSystem == null) return;
        healthSystem.AddHealth(amount);
    }
}
