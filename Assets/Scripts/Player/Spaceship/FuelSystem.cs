using UnityEngine;
using UnityEngine.InputSystem;

public class FuelSystem : MonoBehaviour
{
    [SerializeField]
    public float maxFuel;
    public float currentFuel { get; private set; }
    [SerializeField]
    private float fuelUsageAmount;

    public bool HasFuel => currentFuel > 0f;
    public float FuelPercent => currentFuel / maxFuel;

    #region Input action
    private InputAction moveAction;

    private void OnDisable()
    {
        moveAction?.Disable();
    }
    #endregion

    void Start()
    {
        currentFuel = maxFuel;
    }

    private void FixedUpdate()
    {
        ConsumeFuelOnMove(fuelUsageAmount);
    }

    public void ConsumeFuelOnMove(float amount)
    {
        if (moveAction.ReadValue<Vector2>().magnitude != 0)
        {
            currentFuel = Mathf.Max(0, currentFuel - amount/20);

        }
    }

    public void PlayerMovementInit(InputAction newMoveAction)
    {
        newMoveAction.Enable();
        moveAction = newMoveAction;
    }

    public void Refuel(float amount)
    {
        currentFuel = Mathf.Min(maxFuel, currentFuel + amount);
    }
}
