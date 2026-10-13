using TMPro;
using UnityEngine;

/// <summary>
/// Stasis Core HUD: the run objective (cores carried out of those the Warp Gate requires), warnings
/// (burning a gate key, gate locked, cores missing before the last hazard zone) and a one-time hint
/// explaining Stasis on the first pickup.
/// </summary>
public class StasisCoreHUD : MonoBehaviour
{
    [Header("Texts")]
    [SerializeField] private TMP_Text objectiveText;
    [SerializeField] private TMP_Text warningText;
    [SerializeField] private TMP_Text hintText;

    [Header("References")]
    [SerializeField] private StasisCoreInventory inventory;
    [SerializeField] private WarpGate warpGate;
    [Tooltip("Last hazard zone holding a core along the sector. Approaching it without enough cores shows a warning.")]
    [SerializeField] private Transform lastHazardZone;

    [Header("Timing")]
    [Tooltip("Distance along the sector (Z) before the last hazard zone at which the missing-cores warning appears.")]
    [SerializeField] private float missingCoresLeadDistance = 200f;
    [SerializeField] private float gateLockedMessageDuration = 3f;
    [SerializeField] private float hintDuration = 8f;
    [Tooltip("Number of segments in the key burn hold bar.")]
    [SerializeField] private int holdBarSegments = 10;

    private float gateLockedTimer;
    private float hintTimer;

    private void OnEnable()
    {
        if (inventory != null)
        {
            inventory.OnCountChanged += HandleCountChanged;
            inventory.OnCoreCollected += HandleCoreCollected;
        }
        if (warpGate != null) warpGate.OnEntryDenied += HandleGateEntryDenied;
    }

    private void OnDisable()
    {
        if (inventory != null)
        {
            inventory.OnCountChanged -= HandleCountChanged;
            inventory.OnCoreCollected -= HandleCoreCollected;
        }
        if (warpGate != null) warpGate.OnEntryDenied -= HandleGateEntryDenied;
    }

    private void Start()
    {
        objectiveText.enabled = inventory != null;
        warningText.enabled = false;
        hintText.enabled = false;
        if (inventory != null) HandleCountChanged(inventory.Count);
    }

    private void HandleCountChanged(int count)
    {
        objectiveText.text = inventory.KeysDelivered
            ? $"GATE UNLOCKED  –  SPARE CORES {count}"
            : $"CORES {count}/{inventory.RequiredCores}  –  REACH THE GATE";
    }

    private void HandleCoreCollected()
    {
        // One-time hint per run: only the first pickup explains the double role of a core.
        if (inventory.Collected != 1) return;

        hintText.text =
            $"STASIS CORE  –  the Warp Gate needs {inventory.RequiredCores} as the key.\n" +
            $"Hold [SPACE] to burn one instead: the wave freezes for {inventory.StasisDuration:F0} s.";
        hintTimer = hintDuration;
        hintText.enabled = true;
    }

    private void HandleGateEntryDenied()
    {
        gateLockedTimer = gateLockedMessageDuration;
    }

    private void Update()
    {
        if (inventory == null) return;

        UpdateHint();
        gateLockedTimer = Mathf.Max(0f, gateLockedTimer - Time.deltaTime);

        string warning = PickWarning();
        warningText.enabled = warning != null;
        if (warning != null) warningText.text = warning;
    }

    private void UpdateHint()
    {
        if (hintTimer <= 0f) return;

        hintTimer -= Time.deltaTime;
        if (hintTimer <= 0f) hintText.enabled = false;
    }

    // Most urgent first: an action in progress, then the gate refusing entry, then the planning warning.
    private string PickWarning()
    {
        if (inventory.IsHoldingKeyBurn)
        {
            int filled = Mathf.RoundToInt(inventory.KeyBurnHoldProgress * holdBarSegments);
            string bar = new string('#', filled) + new string('-', holdBarSegments - filled);
            return $"BURNING A GATE KEY  [{bar}]\nKEEP HOLDING TO FREEZE THE WAVE";
        }

        if (inventory.KeysDelivered || inventory.HasRequiredCores) return null;

        string cores = $"{inventory.Count}/{inventory.RequiredCores}";
        if (gateLockedTimer > 0f)
            return $"GATE LOCKED  –  CORES {cores}";

        if (lastHazardZone != null &&
            inventory.transform.position.z >= lastHazardZone.position.z - missingCoresLeadDistance)
            return $"CORES MISSING  {cores}  –  THE GATE WILL NOT OPEN";

        return null;
    }
}
