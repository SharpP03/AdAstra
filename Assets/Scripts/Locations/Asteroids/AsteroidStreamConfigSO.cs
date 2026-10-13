using UnityEngine;

/// <summary>
/// Volume, impulses and motion limits of an asteroid stream: a flat river of asteroids across the route
/// in which random asteroids light up and get kicked, mostly along the river.
/// </summary>
[CreateAssetMenu(fileName = "AsteroidStream_Config", menuName = "AdAstra/Locations/Asteroid Stream Config", order = 51)]
public class AsteroidStreamConfigSO : ScriptableObject
{
    public GameObject[] asteroidPrefabs;

    [Tooltip("Seed of the layout and the impulses. 0 = different every run.")]
    public int seed = 0;

    [Header("Volume")]
    [Tooltip("Size of the box the asteroids stay in (local X = across the route, Z = along it).")]
    public Vector3 size = new Vector3(300f, 120f, 70f);
    [Min(0)] public int count = 80;
    [Tooltip("Scale range of the asteroids (prefabs are 2-4 m in radius at scale 1).")]
    public Vector2 scale = new Vector2(0.8f, 3f);
    [Tooltip("Free radius kept around the center at spawn, where the optional Stasis Core sits.")]
    [Min(0f)] public float coreClearance = 10f;

    [Header("Impulses")]
    [Tooltip("Seconds between two impulses of one asteroid.")]
    public Vector2 impulseInterval = new Vector2(3f, 9f);
    [Tooltip("Speed change of one impulse in m/s.")]
    public Vector2 impulseSpeed = new Vector2(8f, 16f);
    [Tooltip("0 = impulses in any direction, 1 = only along local X (back and forth across the route).")]
    [Range(0f, 1f)] public float axisBias = 0.6f;
    [Tooltip("Seconds the asteroid glows before the impulse.")]
    [Min(0f)] public float telegraphTime = 0.7f;
    [ColorUsage(false, true)]
    public Color telegraphColor = new Color(1f, 0.45f, 0.1f) * 6f;

    [Header("Motion limits")]
    [Tooltip("Acceleration in m/s² that turns an asteroid back once it leaves the box.")]
    [Min(0f)] public float returnAcceleration = 6f;
    [Tooltip("Linear damping: asteroids slow down between impulses.")]
    [Min(0f)] public float damping = 0.3f;
    [Tooltip("Speed limit in m/s, in case impulses stack up.")]
    [Min(0f)] public float maxSpeed = 20f;

    [Header("Physics")]
    [Tooltip("Mass per cubic meter of asteroid volume. The player ship weighs 100.")]
    [Min(0.01f)] public float density = 0.5f;
    [Tooltip("Range of the spin in rad/s.")]
    public Vector2 spinSpeed = new Vector2(0.1f, 0.6f);

    private void OnValidate()
    {
        impulseInterval.x = Mathf.Max(0.1f, impulseInterval.x);
        impulseInterval.y = Mathf.Max(impulseInterval.x, impulseInterval.y);
        impulseSpeed.y = Mathf.Max(impulseSpeed.x, impulseSpeed.y);
    }
}
