using UnityEngine;

public class FuelSystem : MonoBehaviour
{
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
                currentFuel = Mathf.Max(0, currentFuel - config.fuelBurnRate * Time.deltaTime);
                break;
            case PlayerState.Sprinting:
                currentFuel = Mathf.Max(0, currentFuel - config.fuelBurnRate * config.boostFuelMultiplier * Time.deltaTime);
                break;
            default: break;

        }
    }

    public void AddFuel(float amount)
    {
        currentFuel = Mathf.Min(config.maxFuel, currentFuel + amount);
    }
}
