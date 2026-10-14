using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Gravity anomaly with readable rings. The pull is purely radial and constant within a ring:
/// the edge can be escaped with normal thrust, the inner ring only with boost, and the core is a point
/// of no return that spits bodies back toward the Annihilation Wave. A ship leaving the inner ring
/// is launched along its flight direction (slingshot), the deeper it went, the harder.
/// A Stasis Core orbits in the middle of the inner ring.
/// </summary>
[SelectionBase]
public class GravityAnomaly : MonoBehaviour
{
    [Header("Rings")]
    [SerializeField] private float edgeRadius = 300f;
    [SerializeField] private float innerRadius = 180f;
    [SerializeField] private float coreRadius = 50f;
    [Tooltip("Pull at the edge (m/s²); must stay below the ship's thrust so normal flight escapes.")]
    [SerializeField] private float edgePull = 6f;
    [Tooltip("Pull in the inner ring (m/s²); between thrust and boost, so only boost escapes.")]
    [SerializeField] private float innerPull = 18f;
    [SerializeField] private LayerMask affectedLayers = ~0;

    [Header("Exit slingshot (ship only)")]
    [Tooltip("Speed added when leaving the inner ring right at its border.")]
    [SerializeField] private float minSlingshotSpeed = 10f;
    [Tooltip("Speed added when leaving the inner ring after touching the core border.")]
    [SerializeField] private float maxSlingshotSpeed = 40f;
    [SerializeField] private float slingshotLowDragSeconds = 3f;

    [Header("Core spit")]
    [Tooltip("World direction of the spit: back along the sector axis, toward the wave.")]
    [SerializeField] private Vector3 spitDirection = Vector3.back;
    [SerializeField] private float spitSpeed = 60f;
    [SerializeField] [Range(0f, 1f)] private float spitHullDamage = 0.3f;

    [Header("Orbiting Stasis Core")]
    [Tooltip("Optional child moved along a horizontal orbit; it may be destroyed once collected.")]
    [SerializeField] private Transform orbitingCore;
    [SerializeField] private float orbitRadius = 115f;
    [SerializeField] private float orbitPeriod = 60f;

    // Deepest point reached in the inner ring (0 = its border, 1 = core border), per body inside it.
    private readonly Dictionary<Rigidbody, float> innerRingDepth = new();
    // Bodies spat out by the core; ignored until they leave the zone.
    private readonly HashSet<Rigidbody> spatOut = new();
    private readonly HashSet<Rigidbody> seenThisStep = new();
    private readonly List<Rigidbody> stale = new();
    private readonly Collider[] overlapBuffer = new Collider[64];

    private void OnDisable()
    {
        innerRingDepth.Clear();
        spatOut.Clear();
    }

    private void Update()
    {
        if (orbitingCore == null) return;
        float angle = Time.time / Mathf.Max(1f, orbitPeriod) * 2f * Mathf.PI;
        orbitingCore.localPosition = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * orbitRadius;
    }

    private void FixedUpdate()
    {
        Vector3 center = transform.position;
        int count = Physics.OverlapSphereNonAlloc(center, edgeRadius, overlapBuffer, affectedLayers,
            QueryTriggerInteraction.Ignore);

        seenThisStep.Clear();
        for (int i = 0; i < count; i++)
        {
            Rigidbody body = overlapBuffer[i].attachedRigidbody;
            // A ship has several colliders; handle each body once per step.
            if (body == null || body.isKinematic || !seenThisStep.Add(body)) continue;
            if (spatOut.Contains(body)) continue;

            Vector3 toCenter = center - body.position;
            float distance = toCenter.magnitude;

            if (distance < coreRadius)
            {
                SpitOut(body);
                continue;
            }

            Vector3 pullDirection = toCenter / distance;
            if (distance < innerRadius)
            {
                body.AddForce(pullDirection * innerPull, ForceMode.Acceleration);
                float depth = Mathf.InverseLerp(innerRadius, coreRadius, distance);
                innerRingDepth[body] = innerRingDepth.TryGetValue(body, out float deepest) ? Mathf.Max(deepest, depth) : depth;
            }
            else
            {
                body.AddForce(pullDirection * edgePull, ForceMode.Acceleration);
                if (innerRingDepth.Remove(body, out float deepest)) LeaveInnerRing(body, deepest);
            }
        }

        ForgetBodiesOutside();
    }

    private void LeaveInnerRing(Rigidbody body, float deepest)
    {
        #region EDUCATION NOTE wyrzut z anomalii
        // DO PRACY INŻ. - pole grawitacyjne jest zachowawcze: ciało, które wyleci z pierścienia, odzyskuje
        // dokładnie tyle energii, ile straciło przy wlocie, więc "darmowy" dystans nie powstaje sam z siebie.
        // Wyrzut to efekt zaprojektowany: nagroda za ryzyko rośnie z głębokością zejścia. Sam impuls prędkości
        // wygasłby przy oporze silników po ok. 2 s, dlatego statek dostaje też kilka sekund niskiego oporu.
        #endregion
        if (!body.TryGetComponent(out Player_Spaceship ship)) return;
        ship.Slingshot(Mathf.Lerp(minSlingshotSpeed, maxSlingshotSpeed, deepest), slingshotLowDragSeconds, deepest);
    }

    private void SpitOut(Rigidbody body)
    {
        innerRingDepth.Remove(body);
        spatOut.Add(body);
        body.linearVelocity = spitDirection.normalized * spitSpeed;

        if (body.TryGetComponent(out HealthSystem hull))
            hull.TakeDamage(hull.MaxHealth * spitHullDamage);
    }

    private void ForgetBodiesOutside()
    {
        stale.Clear();
        foreach (Rigidbody body in spatOut)
            if (body == null || !seenThisStep.Contains(body)) stale.Add(body);
        foreach (Rigidbody body in innerRingDepth.Keys)
            if (body == null || !seenThisStep.Contains(body)) stale.Add(body);

        foreach (Rigidbody body in stale)
        {
            spatOut.Remove(body);
            innerRingDepth.Remove(body);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 center = transform.position;
        Gizmos.color = new Color(0.3f, 0.6f, 1f, 0.8f);
        Gizmos.DrawWireSphere(center, edgeRadius);
        Gizmos.color = new Color(1f, 0.5f, 0.1f, 0.8f);
        Gizmos.DrawWireSphere(center, innerRadius);
        Gizmos.color = new Color(1f, 0.1f, 0.1f, 0.8f);
        Gizmos.DrawWireSphere(center, coreRadius);
    }
}
