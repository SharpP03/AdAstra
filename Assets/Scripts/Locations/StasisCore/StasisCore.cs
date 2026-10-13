using UnityEngine;

/// <summary>
/// Collectible Stasis Core lying inside a hazard zone. Flying through it adds a core to the ship
/// and repairs part of the hull. Its light pulses so it can be spotted from afar.
/// </summary>
[SelectionBase]
[RequireComponent(typeof(Collider))]
public class StasisCore : MonoBehaviour
{
    [Header("Pulse")]
    [SerializeField] private Light pulseLight;
    [Tooltip("Visual that breathes in scale together with the light.")]
    [SerializeField] private Transform visual;
    [SerializeField] private float pulseFrequency = 1.2f;
    [Tooltip("Relative change of light intensity at the peak of the pulse (0.5 = ±50%).")]
    [SerializeField] private float lightPulseAmount = 0.5f;
    [Tooltip("Relative change of the visual's scale at the peak of the pulse.")]
    [SerializeField] private float scalePulseAmount = 0.12f;
    [SerializeField] private float spinSpeed = 30f;

    private float baseIntensity;
    private Vector3 baseScale;
    private bool collected;

    private void Awake()
    {
        GetComponent<Collider>().isTrigger = true;
        if (pulseLight != null) baseIntensity = pulseLight.intensity;
        if (visual != null) baseScale = visual.localScale;
    }

    private void Update()
    {
        float pulse = Mathf.Sin(Time.time * pulseFrequency * 2f * Mathf.PI);
        if (pulseLight != null) pulseLight.intensity = baseIntensity * (1f + pulse * lightPulseAmount);
        if (visual != null)
        {
            visual.localScale = baseScale * (1f + pulse * scalePulseAmount);
            visual.Rotate(Vector3.up, spinSpeed * Time.deltaTime, Space.Self);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // The ship has several colliders, and Destroy only takes effect at the end of the frame.
        if (collected) return;

        Rigidbody body = other.attachedRigidbody;
        if (body == null || !body.TryGetComponent(out StasisCoreInventory inventory)) return;
        if (!inventory.TryCollect()) return;

        collected = true;

        // Entering the zone partly "pays back" the hull damage taken on the way to the core.
        if (body.TryGetComponent(out HealthRefillable hull) && inventory.Config != null)
            hull.Add(hull.Max * inventory.Config.hullRepairFraction);

        Destroy(gameObject);
    }
}
