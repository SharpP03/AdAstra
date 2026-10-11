using UnityEngine;
using UnityEngine.UI;

public class UI_Manager : MonoBehaviour
{
    private FuelSystem fuelSystem;
    private HealthSystem healthSystem;
    private HullCollisionDamage hullImpacts;
    [SerializeField]
    private Slider fuelBar;
    [SerializeField]
    private Slider healthBar;
    //private Canvas playerUI;

    [Header("Impact flash")]
    [SerializeField] private Color impactFlashColor = Color.white;
    [SerializeField] private float impactFlashDuration = 0.35f;

    private Image healthFill;
    private Color healthFillColor;
    private float flashTimer;

    private void Awake()
    {
        AssignSystems();
        InitializeBarValues();
    }

    private void OnEnable()
    {
        if (hullImpacts != null) hullImpacts.OnImpact += HandleImpact;
    }

    private void OnDisable()
    {
        if (hullImpacts != null) hullImpacts.OnImpact -= HandleImpact;
    }

    private void AssignSystems() {
        fuelSystem = GetComponent<FuelSystem>();
        healthSystem = GetComponent<HealthSystem>();
        hullImpacts = GetComponent<HullCollisionDamage>();
        healthFill = healthBar.fillRect.GetComponent<Image>();
        healthFillColor = healthFill.color;
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
        UpdateImpactFlash();
    }

    private void HandleImpact(float strength)
    {
        flashTimer = impactFlashDuration;
    }

    private void UpdateImpactFlash()
    {
        if (flashTimer <= 0f) return;

        flashTimer = Mathf.Max(0f, flashTimer - Time.deltaTime);
        // Ease-out: the flash snaps on and fades quickly back to the bar's own color.
        float t = flashTimer / impactFlashDuration;
        healthFill.color = Color.Lerp(healthFillColor, impactFlashColor, t * t);
    }

}
