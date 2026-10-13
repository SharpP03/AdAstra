using TMPro;
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

    [Header("Repair feedback")]
    [Tooltip("\"+X\" shown next to the hull bar when the hull is repaired.")]
    [SerializeField] private TMP_Text repairText;
    [SerializeField] private Color repairFlashColor = new Color(0.4f, 1f, 0.5f);
    [SerializeField] private float repairPopupDuration = 1.2f;
    [SerializeField] private float repairPopupRise = 18f;

    private Image healthFill;
    private Color healthFillColor;
    private float flashTimer;
    private Color flashColor;
    private float repairTimer;
    private float repairShown;
    private Vector2 repairTextPosition;

    private void Awake()
    {
        AssignSystems();
        InitializeBarValues();
    }

    private void OnEnable()
    {
        if (hullImpacts != null) hullImpacts.OnImpact += HandleImpact;
        if (healthSystem != null) healthSystem.OnRepaired += HandleRepaired;
    }

    private void OnDisable()
    {
        if (hullImpacts != null) hullImpacts.OnImpact -= HandleImpact;
        if (healthSystem != null) healthSystem.OnRepaired -= HandleRepaired;
    }

    private void AssignSystems() {
        fuelSystem = GetComponent<FuelSystem>();
        healthSystem = GetComponent<HealthSystem>();
        hullImpacts = GetComponent<HullCollisionDamage>();
        healthFill = healthBar.fillRect.GetComponent<Image>();
        healthFillColor = healthFill.color;
        if (repairText != null)
        {
            repairTextPosition = repairText.rectTransform.anchoredPosition;
            repairText.alpha = 0f;
        }
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
        UpdateRepairPopup();
    }

    private void HandleImpact(float strength, Vector3 point, Vector3 normal)
    {
        flashColor = impactFlashColor;
        flashTimer = impactFlashDuration;
    }

    private void HandleRepaired(float amount)
    {
        flashColor = repairFlashColor;
        flashTimer = impactFlashDuration;

        if (repairText == null) return;
        // Repairs in quick succession add up in one popup instead of overwriting each other.
        repairShown = repairTimer > 0f ? repairShown + amount : amount;
        repairTimer = repairPopupDuration;
        repairText.text = $"+{Mathf.RoundToInt(repairShown)}";
    }

    private void UpdateImpactFlash()
    {
        if (flashTimer <= 0f) return;

        flashTimer = Mathf.Max(0f, flashTimer - Time.deltaTime);
        // Ease-out: the flash snaps on and fades quickly back to the bar's own color.
        float t = flashTimer / impactFlashDuration;
        healthFill.color = Color.Lerp(healthFillColor, flashColor, t * t);
    }

    private void UpdateRepairPopup()
    {
        if (repairText == null || repairTimer <= 0f) return;

        repairTimer = Mathf.Max(0f, repairTimer - Time.deltaTime);
        float t = 1f - repairTimer / repairPopupDuration;
        // Holds full opacity for the first half, then fades while drifting up.
        repairText.alpha = Mathf.Clamp01(2f - 2f * t);
        repairText.rectTransform.anchoredPosition = repairTextPosition + Vector2.up * (repairPopupRise * t);
    }

}
