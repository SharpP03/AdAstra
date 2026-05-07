using UnityEngine;
using System.Collections;

public class HealthSystem : MonoBehaviour
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

    public void PlayerTakeDamage(float damageValue)
    {
        currentHealth = currentHealth - damageValue;
    }

    public void AddHealth(float amount)
    {
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
    }
}