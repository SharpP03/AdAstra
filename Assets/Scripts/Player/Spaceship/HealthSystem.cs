using UnityEngine;

public class HealthSystem : MonoBehaviour, IDamageable
{
    [SerializeField]
    private float maxHealth = 300f;
    public float MaxHealth => maxHealth; //readonly ref
    public float currentHealth { get; private set; }

    void Start()
    {
        currentHealth = maxHealth;

    }

    void Update()
    {

    }

    public void TakeDamage(float damageValue)
    {
        currentHealth = currentHealth - damageValue;
    }

    public void PlayerTakeDamage(float damageValue)
    {
        TakeDamage(damageValue);
    }

    public void AddHealth(float amount)
    {
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
    }
}
