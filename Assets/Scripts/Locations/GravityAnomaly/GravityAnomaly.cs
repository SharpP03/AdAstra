using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Gravity anomaly with readable rings. The pull is purely radial and constant within a ring:
/// at the edge normal thrust can stop the ship, in the inner ring only boost can, and the core is a point
/// of no return that spits bodies back toward the Annihilation Wave. Inside the zone the ship has no engine drag,
/// so a tangential flyby speeds it up (slingshot) and a boost at the lowest point pays off most.
/// A Stasis Core orbits in the middle of the inner ring.
/// </summary>
[SelectionBase]
public class GravityAnomaly : MonoBehaviour
{
    [Header("Rings")]
    [SerializeField] private float edgeRadius = 300f;
    [SerializeField] private float innerRadius = 180f;
    [SerializeField] private float coreRadius = 50f;
    [Tooltip("Pull at the edge (m/s²); below the ship's thrust, so normal flight can stop and back out.")]
    [SerializeField] private float edgePull = 6f;
    [Tooltip("Pull in the inner ring (m/s²); between thrust and boost, so only boost can stop or climb.")]
    [SerializeField] private float innerPull = 18f;
    [SerializeField] private LayerMask affectedLayers = ~0;
    [Tooltip("Above this speed (m/s) the zone adds drag, so circling with thrust cannot speed a body up forever.")]
    [SerializeField] private float softSpeedCap = 110f;
    [Tooltip("Drag (1/s) applied to the speed above the soft cap.")]
    [SerializeField] private float overCapDrag = 0.5f;

    [Header("Slingshot exit (ship only)")]
    [Tooltip("Low drag kept after leaving the zone, so the speed gained in the flyby turns into distance.")]
    [SerializeField] private float slingshotLowDragSeconds = 3f;
    [Tooltip("Exit speed that starts the camera kick and exit FX.")]
    [SerializeField] private float fastExitSpeed = 30f;
    [Tooltip("Exit speed that gives the full camera kick.")]
    [SerializeField] private float fullKickExitSpeed = 90f;
    [Tooltip("Optional FX attached to the ship on a fast exit.")]
    [SerializeField] private GameObject fastExitFx;

    [Header("Core spit")]
    [Tooltip("World direction of the spit: back along the sector axis, toward the wave.")]
    [SerializeField] private Vector3 spitDirection = Vector3.back;
    [Tooltip("Spit direction is randomised up to this angle around spitDirection.")]
    [SerializeField] [Range(0f, 90f)] private float spitConeAngle = 30f;
    [SerializeField] private float spitSpeed = 60f;
    [SerializeField] [Range(0f, 1f)] private float spitHullDamage = 0.3f;
    [Tooltip("Optional FX spawned at the core when it spits a body out.")]
    [SerializeField] private GameObject spitFx;
    [SerializeField] private float fxLifetime = 4f;

    [Header("Orbiting Stasis Core")]
    [Tooltip("Optional child moved along a horizontal orbit; it may be destroyed once collected.")]
    [SerializeField] private Transform orbitingCore;
    [SerializeField] private float orbitRadius = 115f;
    [SerializeField] private float orbitPeriod = 60f;

    // Bodies inside the zone in the previous step, to notice when one leaves.
    private readonly HashSet<Rigidbody> inside = new();
    // Bodies spat out by the core; not pulled until they leave the zone.
    private readonly HashSet<Rigidbody> spatOut = new();
    private readonly HashSet<Rigidbody> seenThisStep = new();
    private readonly List<Rigidbody> left = new();
    private readonly Collider[] overlapBuffer = new Collider[64];

    /// <summary>The player's ship while the anomaly pulls it (not while it is being spat out), else null.</summary>
    public Player_Spaceship ShipInside { get; private set; }

    /// <summary>Pull acceleration at a world position; zero outside the zone and inside the core.</summary>
    public Vector3 PullAt(Vector3 position)
    {
        Vector3 toCenter = transform.position - position;
        float distance = toCenter.magnitude;
        if (distance >= edgeRadius || distance < coreRadius) return Vector3.zero;
        return toCenter / distance * (distance < innerRadius ? innerPull : edgePull);
    }

    /// <summary>True if the position is inside the core (point of no return).</summary>
    public bool IsInCore(Vector3 position) => (position - transform.position).sqrMagnitude < coreRadius * coreRadius;

    private void OnDisable()
    {
        inside.Clear();
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
        ShipInside = null;
        for (int i = 0; i < count; i++)
        {
            Rigidbody body = overlapBuffer[i].attachedRigidbody;
            // A ship has several colliders; handle each body once per step.
            if (body == null || body.isKinematic || !seenThisStep.Add(body)) continue;
            body.TryGetComponent(out Player_Spaceship ship);

            if (spatOut.Contains(body))
            {
                // Low drag carries a spat ship out of the zone instead of stranding it inside.
                if (ship != null) ship.HoldInGravityField(0f);
                continue;
            }

            Vector3 toCenter = center - body.position;
            float distance = toCenter.magnitude;

            if (distance < coreRadius)
            {
                SpitOut(body, ship);
                continue;
            }

            bool inInnerRing = distance < innerRadius;
            body.AddForce(toCenter / distance * (inInnerRing ? innerPull : edgePull), ForceMode.Acceleration);

            float speed = body.linearVelocity.magnitude;
            if (speed > softSpeedCap)
                body.AddForce(-body.linearVelocity / speed * ((speed - softSpeedCap) * overCapDrag), ForceMode.Acceleration);

            if (ship != null)
            {
                // Depth signal: a noticeable step at the inner ring border, growing toward the core.
                float stress = inInnerRing ? Mathf.Lerp(0.3f, 1f, Mathf.InverseLerp(innerRadius, coreRadius, distance)) : 0f;
                ship.HoldInGravityField(stress);
                ShipInside = ship;
            }
        }

        HandleBodiesThatLeft();
    }

    private void HandleBodiesThatLeft()
    {
        left.Clear();
        foreach (Rigidbody body in inside)
            if (!seenThisStep.Contains(body)) left.Add(body);

        foreach (Rigidbody body in left)
        {
            inside.Remove(body);
            bool wasSpat = spatOut.Remove(body);
            if (!wasSpat && body != null && body.TryGetComponent(out Player_Spaceship ship)) LaunchSlingshot(ship, body);
        }

        foreach (Rigidbody body in seenThisStep) inside.Add(body);
    }

    private void LaunchSlingshot(Player_Spaceship ship, Rigidbody body)
    {
        #region EDUCATION NOTE proca i efekt Obertha
        // DO PRACY INŻ. - pole grawitacyjne jest zachowawcze: ciało, które wyleci ze strefy, odzyskuje dokładnie tyle
        // energii, ile straciło przy wlocie. Wewnątrz strefy statek nie ma oporu silników, więc spadając po stycznej
        // rozpędza się przy jądrze i zakręca. Zysk daje efekt Obertha: praca ciągu to F·v, więc boost odpalony
        // w najniższym punkcie przelotu (przy największej prędkości) dodaje więcej energii niż w pustej przestrzeni.
        // Po wylocie statek ma jeszcze kilka sekund niskiego oporu, żeby nadwyżka prędkości zamieniła się w dystans.
        #endregion
        float strength = Mathf.InverseLerp(fastExitSpeed, fullKickExitSpeed, body.linearVelocity.magnitude);
        ship.Slingshot(slingshotLowDragSeconds, strength);
        if (strength > 0f && fastExitFx != null)
        {
            GameObject fx = Instantiate(fastExitFx, body.position, Quaternion.LookRotation(body.linearVelocity), body.transform);
            Destroy(fx, fxLifetime);
        }
    }

    private void SpitOut(Rigidbody body, Player_Spaceship ship)
    {
        spatOut.Add(body);

        Vector3 axis = spitDirection.normalized;
        Vector3 tiltAxis = Vector3.Cross(axis, Random.onUnitSphere);
        Vector3 direction = Quaternion.AngleAxis(Random.Range(0f, spitConeAngle), tiltAxis) * axis;
        body.linearVelocity = direction * spitSpeed;

        if (body.TryGetComponent(out HealthSystem hull))
            hull.TakeDamage(hull.MaxHealth * spitHullDamage);
        if (ship != null) ship.HoldInGravityField(0f);

        if (spitFx != null) Destroy(Instantiate(spitFx, transform.position, Quaternion.LookRotation(direction)), fxLifetime);
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
