using System.Globalization;
using TMPro;
using UnityEngine;

/// <summary>
/// Distance from the ship to the Annihilation Wave, with the Stasis countdown shown above it while the wave is frozen.
/// Hidden once the wave stops for good (jump authorized, victory).
/// </summary>
public class WaveHUD : MonoBehaviour
{
    [SerializeField] private TMP_Text distanceText;
    [SerializeField] private TMP_Text stasisText;
    [SerializeField] private AnnihilationWaveController wave;

    private void OnEnable()
    {
        if (wave == null) return;
        wave.OnDistanceChanged += HandleDistanceChanged;
        wave.OnStasisChanged += HandleStasisChanged;
    }

    private void OnDisable()
    {
        if (wave == null) return;
        wave.OnDistanceChanged -= HandleDistanceChanged;
        wave.OnStasisChanged -= HandleStasisChanged;
    }

    private void Start()
    {
        distanceText.enabled = false;
        stasisText.enabled = false;
    }

    private void HandleDistanceChanged(float distance)
    {
        distanceText.enabled = wave.IsActive;
        distanceText.text = $"WAVE  {Mathf.Max(0f, distance):F0} m";
    }

    private void HandleStasisChanged(bool frozen)
    {
        stasisText.enabled = frozen;
    }

    private void Update()
    {
        // Invariant culture: a decimal point, not the system's comma, matches the English HUD.
        if (stasisText.enabled)
            stasisText.text = $"STASIS  {wave.StasisRemaining.ToString("F1", CultureInfo.InvariantCulture)} s";
    }
}
