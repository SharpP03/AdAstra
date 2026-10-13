/// <summary>
/// Snapshot of a finished run, passed from GameManager to the end-of-run panel.
/// </summary>
public readonly struct RunSummary
{
    public readonly GameState Result;
    public readonly float Duration;             // seconds from run start to its end
    public readonly float HullPercent;          // 0..1
    public readonly float FuelPercent;          // 0..1
    public readonly float ClosestWaveDistance;  // meters, closest the wave got during the run
    public readonly int CoresCollected;
    public readonly int CoresBurned;

    public RunSummary(GameState result, float duration, float hullPercent, float fuelPercent, float closestWaveDistance,
        int coresCollected, int coresBurned)
    {
        Result = result;
        Duration = duration;
        HullPercent = hullPercent;
        FuelPercent = fuelPercent;
        ClosestWaveDistance = closestWaveDistance;
        CoresCollected = coresCollected;
        CoresBurned = coresBurned;
    }
}
