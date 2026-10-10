using TMPro;
using UnityEngine;

/// <summary>
/// Shows the distance from the ship to the Warp Gate. Hidden once the gate captures the ship.
/// </summary>
public class ObjectiveDistanceHUD : MonoBehaviour
{
    [SerializeField] private TMP_Text distanceText;
    [SerializeField] private Transform ship;
    [SerializeField] private WarpGate warpGate;

    private void Update()
    {
        bool visible = ship != null && warpGate != null && !warpGate.IsShipCaptured;
        distanceText.enabled = visible;
        if (!visible) return;

        float distance = Vector3.Distance(ship.position, warpGate.transform.position);
        distanceText.text = $"WARP GATE  {distance:F0} m";
    }
}
