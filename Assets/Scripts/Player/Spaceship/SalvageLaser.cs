using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// The ship's salvage laser (multitool). Holding the Laser action fires a beam from the muzzle.
/// A salvage crate inside the aim-assist box in front of the nose pulls the beam onto itself and gets cut,
/// unless something blocks the line of fire. The laser affects nothing but crates; without a target
/// it fires a short beam with no effect. It is off while ship control is locked or the run is not playing.
/// </summary>
[RequireComponent(typeof(Player_Spaceship))]
public class SalvageLaser : MonoBehaviour
{
    [SerializeField] private SalvageConfigSO config;
    [Tooltip("Point the beam starts from; its forward axis is the aim direction.")]
    [SerializeField] private Transform muzzle;
    [SerializeField] private LineRenderer beam;
    [Tooltip("Layers searched for crates and for obstacles blocking the beam.")]
    [SerializeField] private LayerMask hitMask = ~0;

    public bool IsFiring { get; private set; }
    /// <summary>Crate the beam is locked on, or null.</summary>
    public SalvageCrate Target { get; private set; }

    private Player_Spaceship ship;
    private InputAction laserAction;
    private float beamWidth;
    private readonly Collider[] overlapBuffer = new Collider[64];
    private readonly RaycastHit[] rayBuffer = new RaycastHit[16];

    private Transform Muzzle => muzzle != null ? muzzle : transform;

    private void Awake()
    {
        ship = GetComponent<Player_Spaceship>();
        if (config == null) Debug.LogError("SalvageLaser: missing config.", this);
        if (beam != null)
        {
            beamWidth = beam.widthMultiplier;
            beam.useWorldSpace = true;
            beam.positionCount = 2;
            beam.enabled = false;
        }
    }

    private void Start()
    {
        // Resolved in Start, like the flight actions: PlayerInput sets up and enables its action map on its own.
        laserAction = GetComponent<PlayerInput>().actions.FindAction("Laser");
    }

    private void OnDisable()
    {
        StopFiring();
    }

    private void Update()
    {
        bool canFire = config != null && laserAction != null && ship.ControlsEnabled
            && GameManager.Instance != null && GameManager.Instance.CurrentState == GameState.Playing;
        if (!canFire || !laserAction.IsPressed())
        {
            StopFiring();
            return;
        }

        IsFiring = true;
        Vector3 origin = Muzzle.position;
        Target = FindTarget(origin);

        Vector3 end = origin + Muzzle.forward * config.idleBeamLength;
        bool cutting = false;
        if (Target != null)
        {
            // A blocked beam stops on the obstacle and cuts nothing.
            cutting = TryReachTarget(origin, Target, out end);
            if (cutting) Target.Cut(Time.deltaTime, gameObject, end);
        }

        DrawBeam(origin, end, cutting);
    }

    #region EDUCATION NOTE aim assist box
    // DO PRACY INŻ. - wspomaganie celowania przez zapytanie fizyki (Physics.OverlapBoxNonAlloc) zamiast
    // dokładnego raycastu z dziobu. Kamera jest za statkiem, więc idealne celowanie dziobem w małą,
    // ruchomą skrzynię byłoby frustrujące. Prostopadłościan przed dziobem wybiera skrzynię najbliższą
    // osi lotu, a cel już namierzony ma pierwszeństwo, dopóki jest w strefie (wiązka nie przeskakuje).
    // Wersja NonAlloc korzysta ze stałego bufora - brak alokacji co klatkę (GC).
    #endregion
    private SalvageCrate FindTarget(Vector3 origin)
    {
        Transform aim = Muzzle;
        Vector3 halfExtents = new Vector3(config.assistBoxWidth, config.assistBoxWidth, config.laserRange) * 0.5f;
        Vector3 center = origin + aim.forward * halfExtents.z;
        int count = Physics.OverlapBoxNonAlloc(center, halfExtents, overlapBuffer, aim.rotation, hitMask, QueryTriggerInteraction.Collide);

        SalvageCrate best = null;
        float bestAngle = float.MaxValue;
        for (int i = 0; i < count; i++)
        {
            SalvageCrate crate = overlapBuffer[i].GetComponentInParent<SalvageCrate>();
            if (crate == null || crate.IsOpened) continue;
            if (crate == Target) return crate;

            float angle = Vector3.Angle(aim.forward, crate.transform.position - origin);
            if (angle < bestAngle)
            {
                bestAngle = angle;
                best = crate;
            }
        }
        return best;
    }

    /// <summary>
    /// Casts from the muzzle to the crate. Returns true with the hit point on the crate when the line of fire
    /// is clear, or false with the point where an obstacle stops the beam.
    /// </summary>
    private bool TryReachTarget(Vector3 origin, SalvageCrate crate, out Vector3 end)
    {
        Vector3 toCrate = crate.transform.position - origin;
        float distance = toCrate.magnitude;
        int count = Physics.RaycastNonAlloc(origin, toCrate / distance, rayBuffer, distance, hitMask, QueryTriggerInteraction.Ignore);

        float nearest = float.MaxValue;
        RaycastHit nearestHit = default;
        for (int i = 0; i < count; i++)
        {
            RaycastHit hit = rayBuffer[i];
            if (hit.transform.IsChildOf(transform)) continue; // the ship's own colliders
            if (hit.distance < nearest)
            {
                nearest = hit.distance;
                nearestHit = hit;
            }
        }

        // Nothing hit before the crate's center: the cast started inside the crate or it has no solid collider.
        if (nearestHit.collider == null)
        {
            end = crate.transform.position;
            return true;
        }

        end = nearestHit.point;
        return nearestHit.collider.GetComponentInParent<SalvageCrate>() == crate;
    }

    private void DrawBeam(Vector3 origin, Vector3 end, bool cutting)
    {
        if (beam == null) return;

        beam.enabled = true;
        beam.widthMultiplier = cutting ? beamWidth : beamWidth * config.idleBeamWidthScale;
        beam.SetPosition(0, origin);
        beam.SetPosition(1, end);
    }

    private void StopFiring()
    {
        IsFiring = false;
        Target = null;
        if (beam != null) beam.enabled = false;
    }
}
