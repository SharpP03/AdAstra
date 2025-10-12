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


    void Start()
    {
        currentFuel = maxFuel;
        fuelSprintUsage = fuelUsage * 2;
    }

    private void FixedUpdate()
    {
        ConsumeFuel();
    }


    private void ConsumeFuel()
    {
        //if (playerState == PlayerState.Moving)
        //{
        //    currentFuel = Mathf.Max(0, currentFuel - fuelUsage * Time.deltaTime);
        //}

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

    public void UpdatePlayerState(PlayerState newPlayerState)
    {
        playerState = newPlayerState;
    }

    public void Refuel(float amount)
    {
        currentFuel = Mathf.Min(maxFuel, currentFuel + amount);
    }
}
