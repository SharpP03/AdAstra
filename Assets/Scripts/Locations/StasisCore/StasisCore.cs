using System;
using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>
/// Collectible Stasis Core lying inside a hazard zone. Flying through it adds a core to the ship
/// and repairs part of the hull. Its light pulses so it can be spotted from afar.
/// The pulse and the pickup are exposed so the hazard zone around the core can react to them.
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
    [Header("Star flare")]
    [Tooltip("Optional camera-facing star flare that breathes with the pulse and now and then twinkles bigger.")]
    [SerializeField] private Transform flare;
    [SerializeField] private float flarePulseAmount = 0.15f;
    [Tooltip("Flare scale at the peak of a twinkle.")]
    [SerializeField] private float twinkleScale = 2f;
    [SerializeField] private float twinkleDuration = 0.35f;
    [Tooltip("Random pause between twinkles (seconds).")]
    [SerializeField] private Vector2 twinkleInterval = new Vector2(3f, 6f);

    /// <summary>Current pulse value from -1 to 1, in sync with the light and the visual.</summary>
    public float Pulse { get; private set; }

    /// <summary>Raised once when the ship picks the core up, right before it is destroyed.</summary>
    public event Action OnCollected;

    private float baseIntensity;
    private Vector3 baseScale;
    private Vector3 baseFlareScale;
    private float nextTwinkleTime;
    private bool collected;

    private void Awake()
    {
        GetComponent<Collider>().isTrigger = true;
        if (pulseLight != null) baseIntensity = pulseLight.intensity;
        if (visual != null) baseScale = visual.localScale;
        if (flare != null) baseFlareScale = flare.localScale;
        nextTwinkleTime = Time.time + Random.Range(twinkleInterval.x, twinkleInterval.y);
    }

    private void Update()
    {
        Pulse = Mathf.Sin(Time.time * pulseFrequency * 2f * Mathf.PI);
        if (pulseLight != null) pulseLight.intensity = baseIntensity * (1f + Pulse * lightPulseAmount);
        if (visual != null)
        {
            visual.localScale = baseScale * (1f + Pulse * scalePulseAmount);
            visual.Rotate(Vector3.up, spinSpeed * Time.deltaTime, Space.Self);
        }
        if (flare != null) flare.localScale = baseFlareScale * ((1f + Pulse * flarePulseAmount) * Twinkle());
    }

    // 1 most of the time; a short swell up to twinkleScale at random intervals, so the core catches the eye from afar.
    private float Twinkle()
    {
        float sinceStart = Time.time - nextTwinkleTime;
        if (sinceStart < 0f) return 1f;
        if (sinceStart > twinkleDuration)
        {
            nextTwinkleTime = Time.time + Random.Range(twinkleInterval.x, twinkleInterval.y);
            return 1f;
        }
        return 1f + (twinkleScale - 1f) * Mathf.Sin(sinceStart / twinkleDuration * Mathf.PI);
    }

    private void OnTriggerEnter(Collider other)
    {
        // The ship has several colliders, and Destroy only takes effect at the end of the frame.
        if (collected) return;

        Rigidbody body = other.attachedRigidbody;
        if (body == null || !body.TryGetComponent(out StasisCoreInventory inventory)) return;

        collected = true;
        inventory.Collect();

        // Entering the zone partly "pays back" the hull damage taken on the way to the core.
        if (body.TryGetComponent(out HealthRefillable hull) && inventory.Config != null)
            hull.Add(hull.Max * inventory.Config.hullRepairFraction);

        OnCollected?.Invoke();
        Destroy(gameObject);
    }
}
