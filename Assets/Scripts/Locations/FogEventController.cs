using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class FogEventController : MonoBehaviour
{
    [Header("Timing")] [SerializeField] private float minIntervalSeconds = 10f;

    [SerializeField] private float maxIntervalSeconds = 20f;

    [Header("Damage Event")] [SerializeField]
    private float damageAmount = 10f;

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

    private void OnValidate()
    {
        ValidateTiming();
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

        GameObject rootObject = other.transform.root.gameObject;
        IDamageable damageable = ResolveDamageable(rootObject);
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

    private IEnumerator EventLoop()
    {
        while (true)
        {
            var wait = Random.Range(minIntervalSeconds, maxIntervalSeconds);
            yield return new WaitForSeconds(wait);
            FogEvent();
        }
    }

    private void FogEvent()
    {
        if (!isPlayerInside || currentTargetDamageable == null) return;
        currentTargetDamageable.TakeDamage(damageAmount);
    }

    private void ValidateTiming()
    {
        minIntervalSeconds = Mathf.Max(0.01f, minIntervalSeconds);
        if (maxIntervalSeconds < minIntervalSeconds)
        {
            maxIntervalSeconds = minIntervalSeconds;
        }
    }

    private static IDamageable ResolveDamageable(GameObject rootObject)
    {
        if (rootObject == null) return null;

        MonoBehaviour[] behaviours = rootObject.GetComponentsInChildren<MonoBehaviour>();
        for (int i = 0; i < behaviours.Length; i++)
        {
            if (behaviours[i] is IDamageable damageable)
            {
                return damageable;
            }
        }

        return null;
    }
}
