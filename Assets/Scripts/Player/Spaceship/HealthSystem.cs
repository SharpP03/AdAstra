using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

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
        // TODO: DELETE - temporary debug input to halve HP
        if (Keyboard.current != null && Keyboard.current.digit1Key.wasPressedThisFrame)
        {
            TakeDamage(currentHealth * 0.5f);
        }
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