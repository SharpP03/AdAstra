using System.Collections.Generic;
using UnityEngine;

public class GravityAnomallyController : MonoBehaviour
{
    private readonly Dictionary<Rigidbody, Vector3> _orbitalAxes = new();

    private void Update()
    {
        if (!_useFixedUpdate) PullObjectsToCenter();
    }

    private void FixedUpdate()
    {
        if (_useFixedUpdate) PullObjectsToCenter();
    }

    private void OnEnable()
    {
        _orbitalAxes.Clear();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.2f, 0.6f, 1f, 0.25f);
        Gizmos.DrawSphere(transform.position, _circularRadius);
        Gizmos.color = new Color(0.2f, 0.6f, 1f, 1f);
        Gizmos.DrawWireSphere(transform.position, _circularRadius);

        // Visualize surge zone
        Gizmos.color = new Color(1f, 0.4f, 0.1f, 0.15f);
        Gizmos.DrawSphere(transform.position, _circularRadius * _surgeZoneStart);
        Gizmos.color = new Color(1f, 0.4f, 0.1f, 0.6f);
        Gizmos.DrawWireSphere(transform.position, _circularRadius * _surgeZoneStart);
    }

    private void PullObjectsToCenter()
    {
        var center = transform.position;
        var cols = Physics.OverlapSphere(center, _circularRadius, _affectedLayers);

        for (var i = 0; i < cols.Length; i++)
        {
            var col = cols[i];
            if (col == null) continue;

            var rb = col.attachedRigidbody;
            if (rb == null) continue;
            if (rb.isKinematic && !_includeKinematic) continue;

            var toCenter = center - rb.position;
            var dist = toCenter.magnitude;
            if (dist < 0.0001f) continue;

            var dir = toCenter / dist;
            var normalizedDist = dist / _circularRadius;

            // --- 1. GRAVITY ---
            var gravityFalloff = normalizedDist;
            rb.AddForce(dir * _pullForce * gravityFalloff, ForceMode.Acceleration);

            // --- 2. ORBITAL AXIS ---
            if (!_orbitalAxes.TryGetValue(rb, out var orbitalAxis))
            {
                orbitalAxis = new Vector3(
                    Random.Range(-_orbitTilt, _orbitTilt),
                    1f,
                    Random.Range(-_orbitTilt, _orbitTilt)
                ).normalized;
                _orbitalAxes[rb] = orbitalAxis;
            }

            // Edge proximity (0 = inner zone, 1 = outer edge)
            var edgeProximity = Mathf.Clamp01((normalizedDist - _surgeZoneStart) / (1f - _surgeZoneStart));

            // --- 3. TANGENTIAL FORCE (scaled when outside inner zone) ---
            if (Mathf.Abs(_rotationForce) > 0.0001f)
            {
                var tangent = Vector3.Cross(dir, orbitalAxis);
                if (tangent.sqrMagnitude < 0.001f)
                {
                    orbitalAxis = new Vector3(orbitalAxis.z, orbitalAxis.x, orbitalAxis.y);
                    _orbitalAxes[rb] = orbitalAxis;
                    tangent = Vector3.Cross(dir, orbitalAxis);
                }

                tangent.Normalize();

                // Slow rotation outside the inner zone; scale from 1 to _outsideZoneRotationScale
                var rotationScale = Mathf.Lerp(1f, _outsideZoneRotationScale, edgeProximity);
                rb.AddForce(tangent * _rotationForce * rotationScale, ForceMode.Acceleration);
            }

            // --- 4. EDGE GRAVITY SURGE ---
            if (edgeProximity > 0f)
            {
                var surgeForce = _pullForce * edgeProximity * edgeProximity * _edgeGravitySurge;
                rb.AddForce(dir * surgeForce, ForceMode.Acceleration);
            }

            // Light radial-only damping everywhere to prevent infinite spiraling
            var radialVel = Vector3.Dot(rb.linearVelocity, dir) * dir;
            rb.AddForce(-radialVel * _dampingForce, ForceMode.Acceleration);

            // Additional tangential damping when outside inner zone to slow orbiting objects
            if (edgeProximity > 0f && _tangentialDamping > 0f)
            {
                var tangentialVel = rb.linearVelocity - radialVel;
                rb.AddForce(-tangentialVel * (_tangentialDamping * edgeProximity), ForceMode.Acceleration);
            }
        }
    }

    /// <summary>
    ///     Call this when an object enters the gravity field to give it an initial orbital velocity.
    ///     Without this, objects fall straight into the center.
    /// </summary>
    public void KickIntoOrbit(Rigidbody rb)
    {
        var toCenter = transform.position - rb.position;
        var dist = toCenter.magnitude;
        if (dist < 0.0001f) return;

        var dir = toCenter / dist;
        var orbitSpeed = Mathf.Sqrt(_pullForce * dist);
        var axis = _orbitalAxes.TryGetValue(rb, out var existing) ? existing : Vector3.up;
        var tangent = Vector3.Cross(dir, axis).normalized;
        rb.linearVelocity = tangent * orbitSpeed * _initialKickScale;
    }

    #region Parameters

    [Header("Gravity")] [SerializeField] private float _pullForce = 20f;

    [SerializeField] private float _circularRadius = 5f;
    [SerializeField] private LayerMask _affectedLayers = ~0;
    [SerializeField] private bool _includeKinematic;
    [SerializeField] private bool _useFixedUpdate = true;

    [Header("Orbit")] [SerializeField] private float _rotationForce = 15f;
    [SerializeField] [Range(0f, 1f)] private float _outsideZoneRotationScale = 0.25f;

    [SerializeField] [Range(0f, 1f)] private float _orbitTilt = 0.6f;
    [SerializeField] [Range(0f, 2f)] private float _initialKickScale = 0.75f;

    [Header("Damping")] [SerializeField] private float _dampingForce = 0.3f;
    [SerializeField] private float _tangentialDamping = 1.5f;

    [SerializeField] [Range(0f, 1f)] private float _surgeZoneStart = 0.75f;
    [SerializeField] private float _edgeGravitySurge = 3f;

    #endregion
}