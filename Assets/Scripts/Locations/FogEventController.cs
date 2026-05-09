using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider))]
[SelectionBase]
public class FogEventController : MonoBehaviour
{
    [Header("Timing")] [SerializeField] private float minIntervalSeconds = 10f;

    [SerializeField] private float maxIntervalSeconds = 20f;

    [Header("Damage Event")] [SerializeField]
    private float damageAmount = 10f;

    [SerializeField] private float damageDelaySeconds = 1.5f;

    [Header("Warning SphereCast")] [SerializeField]
    private float warningSphereRadius = 1.5f;

    [SerializeField] private float warningRandomSpreadRadius = 3f;

    [Header("VFX")]
    [SerializeField] private GameObject warningParticlePrefab;
    [SerializeField] private GameObject damageParticlePrefab;

    [Header("References")] [SerializeField]
    private Collider zoneTrigger;

    private GameObject currentTarget;
    private IDamageable currentTargetDamageable;

    private bool isPlayerInside;

    private void Awake()
    {
        ValidateTiming();

        if (zoneTrigger == null) zoneTrigger = GetComponentInChildren<Collider>();

        if (zoneTrigger == null)
            Debug.LogWarning($"FogEventController '{name}' has no trigger collider assigned.");
        else if (!zoneTrigger.isTrigger)
            Debug.LogWarning($"FogEventController '{name}' collider should be marked as Trigger.");
    }

    private void OnEnable()
    {
        StartCoroutine(EventLoop());
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        isPlayerInside = false;
        currentTarget = null;
        currentTargetDamageable = null;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        var rootObject = other.transform.root.gameObject;
        var damageable = ResolveDamageable(rootObject);
        if (damageable == null) return;

        currentTarget = rootObject;
        currentTargetDamageable = damageable;
        isPlayerInside = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (other.transform.root.gameObject == currentTarget)
        {
            currentTarget = null;
            currentTargetDamageable = null;
            isPlayerInside = false;
        }
    }

    private void OnValidate()
    {
        ValidateTiming();
    }

    private IEnumerator EventLoop()
    {
        while (true)
        {
            yield return new WaitUntil(() => isPlayerInside && currentTargetDamageable != null);

            var wait = Random.Range(minIntervalSeconds, maxIntervalSeconds);
            var elapsed = 0f;
            while (elapsed < wait && isPlayerInside && currentTargetDamageable != null)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }

            if (!isPlayerInside || currentTargetDamageable == null) continue;
            FogEvent();
        }
    }

    private void FogEvent()
    {
        if (!isPlayerInside || currentTargetDamageable == null) return;
        var warningPoint = GetWarningPoint(currentTarget.transform.position);
        SpawnWarningEffect(warningPoint);
        StartCoroutine(ApplyDelayedDamage(currentTarget, currentTargetDamageable, warningPoint));
    }

    private void ValidateTiming()
    {
        minIntervalSeconds = Mathf.Max(0.01f, minIntervalSeconds);
        if (maxIntervalSeconds < minIntervalSeconds) maxIntervalSeconds = minIntervalSeconds;
        damageDelaySeconds = Mathf.Max(0f, damageDelaySeconds);
        warningSphereRadius = Mathf.Max(0.05f, warningSphereRadius);
        warningRandomSpreadRadius = Mathf.Max(0f, warningRandomSpreadRadius);
    }

    private IEnumerator ApplyDelayedDamage(GameObject targetRoot, IDamageable damageable, Vector3 warningPoint)
    {
        yield return new WaitForSeconds(damageDelaySeconds);
        if (!isPlayerInside || targetRoot == null || currentTarget != targetRoot || damageable == null) yield break;
        if (!IsTargetInWarningArea(targetRoot, warningPoint)) yield break;

        SpawnDamageEffect(warningPoint);
        damageable.TakeDamage(damageAmount);
    }

    private bool IsTargetInWarningArea(GameObject targetRoot, Vector3 warningPoint)
    {
        var playerPosition = targetRoot.transform.position;
        playerPosition.y = warningPoint.y;
        return (playerPosition - warningPoint).sqrMagnitude <= warningSphereRadius * warningSphereRadius;
    }

    private void SpawnWarningEffect(Vector3 warningPoint)
    {
        SpawnEffect(warningParticlePrefab, warningPoint);
    }

    private void SpawnDamageEffect(Vector3 warningPoint)
    {
        SpawnEffect(damageParticlePrefab, warningPoint);
    }

    private void SpawnEffect(GameObject effectPrefab, Vector3 position)
    {
        if (effectPrefab == null) return;

        var effectInstance = Instantiate(effectPrefab, position, Quaternion.identity);
        var destroyAfter = ResolveEffectLifetime(effectInstance);
        if (destroyAfter > 0f) Destroy(effectInstance, destroyAfter);
    }

    private static float ResolveEffectLifetime(GameObject effectInstance)
    {
        if (effectInstance == null) return 0f;

        var particleSystems = effectInstance.GetComponentsInChildren<ParticleSystem>();
        if (particleSystems.Length == 0) return 5f;

        var longestLifetime = 0f;
        for (var i = 0; i < particleSystems.Length; i++)
        {
            var main = particleSystems[i].main;
            var lifetime = main.duration;
            if (main.startLifetime.mode == ParticleSystemCurveMode.TwoConstants)
                lifetime += main.startLifetime.constantMax;
            else
                lifetime += main.startLifetime.constant;

            if (lifetime > longestLifetime) longestLifetime = lifetime;
        }

        return longestLifetime + 0.5f;
    }

    private Vector3 GetWarningPoint(Vector3 targetPosition)
    {
        var maxOffsetFromPlayer = Mathf.Min(warningRandomSpreadRadius, warningSphereRadius * 0.9f);
        var randomOffset = Random.insideUnitCircle * maxOffsetFromPlayer;
        var spreadPosition = targetPosition + new Vector3(randomOffset.x, 0f, randomOffset.y);
        var castOrigin = spreadPosition + Vector3.up * 2f;

        if (Physics.SphereCast(castOrigin, warningSphereRadius, Vector3.down, out var hit, 4f, ~0,
                QueryTriggerInteraction.Ignore))
            return hit.point;

        return spreadPosition;
    }

    private static IDamageable ResolveDamageable(GameObject rootObject)
    {
        if (rootObject == null) return null;

        var behaviours = rootObject.GetComponentsInChildren<MonoBehaviour>();
        for (var i = 0; i < behaviours.Length; i++)
            if (behaviours[i] is IDamageable damageable)
                return damageable;

        return null;
    }
}
