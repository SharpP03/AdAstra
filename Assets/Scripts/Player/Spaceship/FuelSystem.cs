using System.Collections;
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

    private Player_Spaceship player;

    private IEnumerator WaitForPlayer()
    {
        while (GameManager.Instance == null || GameManager.Instance.Player == null)
            yield return null; // czekaj jedn¹ klatkê

        player = GameManager.Instance.Player;
        currentFuel = maxFuel;
        fuelSprintUsage = fuelUsage * 2.2f;
    }
    void Start()
    {
        StartCoroutine(WaitForPlayer());
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
