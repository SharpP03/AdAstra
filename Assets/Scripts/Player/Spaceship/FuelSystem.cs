using UnityEngine;
using UnityEngine.InputSystem;

public class FuelSystem : MonoBehaviour
{
    [SerializeField]
    public float maxFuel;
    public float currentFuel { get; private set; }
    [SerializeField]
    private float fuelUsage;
    private float fuelSprintUsage;

    public bool HasFuel => currentFuel > 0f;
    public float FuelPercent => currentFuel / maxFuel;

    private PlayerState playerState;
    private Player_Spaceship player;


    void Start()
    {
        currentFuel = maxFuel;
        fuelSprintUsage = fuelUsage * 2;
        player = GetComponent<Player_Spaceship>();
        if (player == null)
        {
            Debug.LogError("ThrusterFXController: Player_Spaceship not found!");
            enabled = false;
            return;
        }
        playerState = player.CurrentState;
    }

    private void FixedUpdate()
    {
        ConsumeFuel();
    }


    private void ConsumeFuel()
    {
        switch (playerState)
        {
            case PlayerState.Moving:
                currentFuel = Mathf.Max(0, currentFuel - fuelUsage * Time.deltaTime);
                break;
            case PlayerState.Sprinting:
                currentFuel = Mathf.Max(0, currentFuel - fuelSprintUsage * Time.deltaTime);
                break;
            default: break;

        }
    }

    public void Refuel(float amount)
    {
        currentFuel = Mathf.Min(maxFuel, currentFuel + amount);
    }
}
