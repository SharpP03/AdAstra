using UnityEngine;

/// <summary>
/// World-space bar showing the station's remaining fuel reserve.
/// Scales its own transform along Y, so place it on a pivot at the bottom of the bar.
/// </summary>
public class StationFuelGauge : MonoBehaviour
{
    [SerializeField] private RefuelStation station;

    private float fullHeight;

    private void Awake()
    {
        fullHeight = transform.localScale.y;
    }

    private void OnEnable()
    {
        if (station != null) station.FuelChanged += Show;
    }

    private void OnDisable()
    {
        if (station != null) station.FuelChanged -= Show;
    }

    private void Start()
    {
        if (station == null)
        {
            Debug.LogError("StationFuelGauge: missing station reference.", this);
            return;
        }

        Show(station.RemainingFraction);
    }

    private void Show(float fraction)
    {
        Vector3 scale = transform.localScale;
        scale.y = fullHeight * Mathf.Clamp01(fraction);
        transform.localScale = scale;
    }
}
