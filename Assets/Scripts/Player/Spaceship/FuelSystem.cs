using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class FuelSystem : MonoBehaviour
{
    [SerializeField]
    private float maxFuel = 100f;
    public float MaxFuel => maxFuel; // export maxFuel readonly
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
            yield return null; // wait for single frame

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

        // TODO: DELETE - temporary debug input to halve HP
        if (Keyboard.current != null && Keyboard.current.digit1Key.wasPressedThisFrame)
        {
            currentFuel = maxFuel/2;
        }

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

    public void AddFuel(float amount)
    {
        currentFuel = Mathf.Min(maxFuel, currentFuel + amount);
    }
}