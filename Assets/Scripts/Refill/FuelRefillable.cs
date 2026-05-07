using UnityEngine;

[RequireComponent(typeof(FuelSystem))]
public class FuelRefillable : MonoBehaviour, IRefillable
{
    private FuelSystem fuelSystem;

    public ResourceKind Kind => ResourceKind.Fuel;
    public float Max => fuelSystem != null ? fuelSystem.MaxFuel : 0f;

    private void Awake()
    {
        fuelSystem = GetComponent<FuelSystem>();
    }

    public void Add(float amount)
    {
        if (fuelSystem == null) return;
        fuelSystem.AddFuel(amount);
    }
}
