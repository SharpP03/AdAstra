using TMPro;
using UnityEngine;

/// <summary>
/// Run objective on the HUD: how many Stasis Cores the ship carries out of those the Warp Gate requires,
/// plus a warning while the player holds Stasis to burn a core that is still a gate key.
/// </summary>
public class StasisCoreHUD : MonoBehaviour
{
    [SerializeField] private TMP_Text objectiveText;
    [SerializeField] private TMP_Text warningText;
    [SerializeField] private StasisCoreInventory inventory;
    [Tooltip("Number of segments in the key burn hold bar.")]
    [SerializeField] private int holdBarSegments = 10;

    private void OnEnable()
    {
        if (inventory == null) return;
        inventory.OnCountChanged += HandleCountChanged;
        inventory.OnKeyBurnHoldChanged += HandleKeyBurnHoldChanged;
    }

    private void OnDisable()
    {
        if (inventory == null) return;
        inventory.OnCountChanged -= HandleCountChanged;
        inventory.OnKeyBurnHoldChanged -= HandleKeyBurnHoldChanged;
    }

    private void Start()
    {
        objectiveText.enabled = inventory != null;
        warningText.enabled = false;
        if (inventory != null) HandleCountChanged(inventory.Count);
    }

    private void HandleCountChanged(int count)
    {
        objectiveText.text = $"CORES {count}/{inventory.RequiredCores}  –  REACH THE GATE";
    }

    private void HandleKeyBurnHoldChanged(bool holding)
    {
        warningText.enabled = holding;
    }

    private void Update()
    {
        if (!warningText.enabled) return;

        int filled = Mathf.RoundToInt(inventory.KeyBurnHoldProgress * holdBarSegments);
        string bar = new string('#', filled) + new string('-', holdBarSegments - filled);
        warningText.text = $"BURNING A GATE KEY  [{bar}]\nKEEP HOLDING TO FREEZE THE WAVE";
    }
}
