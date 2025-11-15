using UnityEngine;
using UnityEngine.UI;

public class UI_Manager : MonoBehaviour
{
    private FuelSystem fuelSystem;
    private HealthSystem healthSystem;
    [SerializeField]
    private Slider fuelBar;
    [SerializeField]
    private Slider healthBar;
    //private Canvas playerUI;

    private void Awake()
    {
        AssignSystems();
        InitializeBarValues();
    }

    private void AssignSystems() {
        fuelSystem = GetComponent<FuelSystem>();
        healthSystem = GetComponent<HealthSystem>();
    }

    private void InitializeBarValues()
    {
        fuelBar.maxValue = fuelSystem.MaxFuel;
        healthBar.maxValue = 
            healthSystem.MaxHealth;
    }

    void Start()
    {

    }

    void Update()
    {
        fuelBar.value = fuelSystem.currentFuel;
        healthBar.value = healthSystem.currentHealth;
    }

}
