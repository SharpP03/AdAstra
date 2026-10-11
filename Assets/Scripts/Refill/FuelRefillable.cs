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

    public float Add(float amount)
    {
        if (fuelSystem == null) return 0f;

        float before = fuelSystem.currentFuel;
        fuelSystem.AddFuel(amount);
        return fuelSystem.currentFuel - before;
    }
}
