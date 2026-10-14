using UnityEngine;

public class FuelSystem : MonoBehaviour
{
#if UNITY_EDITOR
    [ContextMenu("Debug: take 50 fuel")]
    private void DebugConsumeFuel() => ConsumeFuel(50f);

    [ContextMenu("Debug: empty fuel tank")]
    private void DebugEmptyTank() => ConsumeFuel(currentFuel);
#endif

    [Tooltip("Same asset as on Player_Spaceship: max fuel and burn rates.")]
    [SerializeField] private ShipFlightConfigSO config;

    public float MaxFuel => config.maxFuel; // export maxFuel readonly
    public float currentFuel { get; private set; }

    public bool HasFuel => currentFuel > 0f;
    public float FuelPercent => currentFuel / config.maxFuel;

    private Player_Spaceship player;

    private void Awake()
    {
        player = GetComponent<Player_Spaceship>();
        if (config == null)
        {
            Debug.LogError("FuelSystem: missing ShipFlightConfigSO.", this);
            enabled = false;
            return;
        }
        currentFuel = config.maxFuel;
    }

    private void Update()
    {
        ConsumeFuel();
    }


    private void ConsumeFuel()
    {
        if (player == null) return;
        //Debug.Log(currentFuel);

        switch (player.CurrentState)
        {
            case PlayerState.Moving:
                ConsumeFuel(config.fuelBurnRate * Time.deltaTime);
                break;
            case PlayerState.Sprinting:
                ConsumeFuel(config.fuelBurnRate * config.boostFuelMultiplier * Time.deltaTime);
                break;
            default: break;
        }
    }

    public void ConsumeFuel(float amount)
    {
        currentFuel = Mathf.Max(0f, currentFuel - amount);
    }

    public void TakeFuel(float amount) => ConsumeFuel(amount);

    public void AddFuel(float amount)
    {
        currentFuel = Mathf.Min(config.maxFuel, currentFuel + amount);
    }
}
