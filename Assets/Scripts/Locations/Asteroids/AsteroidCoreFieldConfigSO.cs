using UnityEngine;

/// <summary>
/// Layout and motion of an asteroid field guarding a Stasis Core: clusters around big boulders,
/// denser toward a small pocket with the core, asteroids that breathe around their anchors
/// and a field that closes in once the core is taken.
/// </summary>
[CreateAssetMenu(fileName = "AsteroidCoreField_Config", menuName = "AdAstra/Locations/Asteroid Core Field Config", order = 50)]
public class AsteroidCoreFieldConfigSO : ScriptableObject
{
    public GameObject[] asteroidPrefabs;

    [Tooltip("Seed of the layout. 0 = a new layout every run.")]
    public int seed = 0;

    [Header("Shape")]
    [Tooltip("Radius of the field before flattening.")]
    [Min(1f)] public float fieldRadius = 180f;
    [Tooltip("Per-axis scale of the field (local X, Y, Z). Y below 1 flattens it into a disc.")]
    public Vector3 shapeScale = new Vector3(1f, 0.5f, 1f);
    [Tooltip("Radius of the empty chamber with the core. The ship is about 4 x 1.5 x 6.5 m.")]
    [Min(0f)] public float pocketRadius = 15f;
    [Tooltip("Free radius of the entrance that runs from the pocket toward local -Z (the approach side).")]
    [Min(0f)] public float entranceRadius = 8f;

    [Header("Clusters")]
    [Min(0)] public int clusterCount = 26;
    [Tooltip("How strongly clusters and loose asteroids gather toward the pocket. 1 = density falls as 1/r², higher = even tighter around the core.")]
    [Range(1f, 4f)] public float centerBias = 3f;
    [Tooltip("Radius around a cluster center in which its members are placed.")]
    public Vector2 clusterRadius = new Vector2(14f, 30f);
    [Tooltip("Number of members around the boulder of one cluster.")]
    public Vector2Int clusterMembers = new Vector2Int(6, 14);
    [Tooltip("Scale range of the boulder in the middle of a cluster (prefabs are 2-4 m in radius at scale 1).")]
    public Vector2 boulderScale = new Vector2(5f, 9f);

    [Header("Loose asteroids")]
    [Tooltip("Asteroids scattered through the whole field outside the clusters.")]
    [Min(0)] public int looseCount = 150;

    [Header("Sizes")]
    [Tooltip("Scale range of cluster members and loose asteroids.")]
    public Vector2 memberScale = new Vector2(0.6f, 2.5f);
    [Tooltip("Exponent of the size distribution. Above 1: many small asteroids and few big ones.")]
    [Range(1f, 5f)] public float sizeBias = 2.5f;

    [Header("Spacing")]
    [Tooltip("Minimum free space between the surfaces of two asteroids at their anchors.")]
    [Min(0f)] public float minClearance = 1.5f;
    [Tooltip("Attempts to find a free spot for one asteroid before it is skipped.")]
    [Range(1, 50)] public int placementAttempts = 20;

    [Header("Breathing")]
    [Tooltip("Wander amplitude in meters at the edge (x) and at the pocket (y).")]
    public Vector2 wanderAmplitude = new Vector2(0.5f, 3f);
    [Tooltip("Seconds of one slow wander cycle.")]
    [Min(0.1f)] public float wanderPeriod = 4f;
    [Tooltip("Radial push in meters in rhythm with the core pulse, at the edge (x) and at the pocket (y).")]
    public Vector2 pulseAmplitude = new Vector2(0f, 0.4f);
    [Tooltip("Seconds an asteroid needs to swing back to its target. Lower = stiffer spring.")]
    [Min(0.1f)] public float springPeriod = 1.5f;
    [Tooltip("Spring damping ratio. 1 = returns without overshoot.")]
    [Range(0f, 2f)] public float springDamping = 0.7f;

    [Header("Collapse")]
    [Tooltip("Fraction of the distance to the center the anchors move once the core is taken.")]
    [Range(0f, 1f)] public float collapseFraction = 0.5f;
    [Tooltip("Seconds the collapse takes.")]
    [Min(0.1f)] public float collapseDuration = 15f;

    [Header("Physics")]
    [Tooltip("Mass per cubic meter of asteroid volume. The player ship weighs 100.")]
    [Min(0.01f)] public float density = 0.5f;
    [Tooltip("Range of the spin in rad/s.")]
    public Vector2 spinSpeed = new Vector2(0.05f, 0.3f);

    private void OnValidate()
    {
        pocketRadius = Mathf.Min(pocketRadius, fieldRadius);
        clusterMembers.x = Mathf.Max(0, clusterMembers.x);
        clusterMembers.y = Mathf.Max(clusterMembers.x, clusterMembers.y);
    }
}
