using TMPro;
using UnityEngine;

/// <summary>
/// Run objective on the HUD: how many Stasis Cores the ship carries out of those the Warp Gate requires.
/// </summary>
public class StasisCoreHUD : MonoBehaviour
{
    [SerializeField] private TMP_Text objectiveText;
    [SerializeField] private StasisCoreInventory inventory;

    private void OnEnable()
    {
        if (inventory != null) inventory.OnCountChanged += HandleCountChanged;
    }

    private void OnDisable()
    {
        if (inventory != null) inventory.OnCountChanged -= HandleCountChanged;
    }

    private void Start()
    {
        objectiveText.enabled = inventory != null;
        if (inventory != null) HandleCountChanged(inventory.Count);
    }

    private void HandleCountChanged(int count)
    {
        objectiveText.text = $"CORES {count}/{inventory.RequiredCores}  –  REACH THE GATE";
    }
}
