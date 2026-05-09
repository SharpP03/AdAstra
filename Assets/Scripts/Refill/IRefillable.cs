public interface IRefillable
{
    ResourceKind Kind { get; }
    float Max { get; }
    void Add(float amount);
}
