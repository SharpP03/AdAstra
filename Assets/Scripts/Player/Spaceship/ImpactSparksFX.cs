using UnityEngine;

/// <summary>
/// Spawns a burst of sparks at the contact point of every damaging impact.
/// The spark count scales with the impact strength.
/// </summary>
public class ImpactSparksFX : MonoBehaviour
{
    [SerializeField] private ParticleSystem sparksPrefab;
    [SerializeField] private int minSparks = 8;
    [SerializeField] private int maxSparks = 60;

    private HullCollisionDamage hullImpacts;

    private void Awake()
    {
        hullImpacts = GetComponent<HullCollisionDamage>();
    }

    private void OnEnable()
    {
        if (hullImpacts != null) hullImpacts.OnImpact += HandleImpact;
    }

    private void OnDisable()
    {
        if (hullImpacts != null) hullImpacts.OnImpact -= HandleImpact;
    }

    private void HandleImpact(float strength, Vector3 point, Vector3 normal)
    {
        if (sparksPrefab == null) return;

        // The prefab emits nothing on its own; it plays once and destroys itself (Stop Action = Destroy).
        ParticleSystem sparks = Instantiate(sparksPrefab, point, Quaternion.LookRotation(normal));
        sparks.Emit(Mathf.RoundToInt(Mathf.Lerp(minSparks, maxSparks, strength)));
    }
}
