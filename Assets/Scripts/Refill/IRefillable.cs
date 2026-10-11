public interface IRefillable
{
    ResourceKind Kind { get; }
    float Max { get; }

    /// <summary>Adds up to <paramref name="amount"/> and returns how much was actually accepted.</summary>
    float Add(float amount);
}
