using UnityEngine;

public class GravityAnomallyController : MonoBehaviour
{
    private void Update()
    {
        if (!_useFixedUpdate) PullObjectsToCenter();
    }

    private void FixedUpdate()
    {
        if (_useFixedUpdate) PullObjectsToCenter();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.2f, 0.6f, 1f, 0.25f);
        Gizmos.DrawSphere(transform.position, _circularRadius);
        Gizmos.color = new Color(0.2f, 0.6f, 1f, 1f);
        Gizmos.DrawWireSphere(transform.position, _circularRadius);
    }

    private void PullObjectsToCenter()
    {
        var center = transform.position;
        Collider[] cols = Physics.OverlapSphere(center, _circularRadius, _affectedLayers);

        for (var i = 0; i < cols.Length; i++)
        {
            var col = cols[i];
            if (col == null) continue;

            var rb = col.attachedRigidbody;

            if (rb != null)
            {
                if (rb.isKinematic && !_includeKinematic) continue;

                var toCenter = center - rb.position;
                var dist = toCenter.magnitude;
                if (dist < 0.0001f) continue;

                var dir = toCenter / dist;

                // falloff so objects nearer get less additional pull
                var falloff = 1f - Mathf.Clamp01(dist / _circularRadius);
                var strength = _pullForce * falloff;

                // Apply pull (Acceleration makes it mass-independent)
                rb.AddForce(dir * strength, ForceMode.Acceleration);

                // optional tangential (orbit) force perpendicular to radial direction
                if (Mathf.Abs(_rotationForce) > 0.0001f)
                {
                    var up = Vector3.up;
                    // if radial is nearly vertical pick another axis to cross with to avoid zero vector
                    if (Mathf.Abs(Vector3.Dot(dir, up)) > 0.99f) up = transform.right;
                    var tangent = Vector3.Cross(dir, up).normalized;
                    var rotStrength = _rotationForce * falloff;
                    rb.AddForce(tangent * rotStrength, ForceMode.Acceleration);
                }
            }
            else
            {
                // No rigidbody: gently move transform toward center (non-physics)
                var t = col.transform;
                var toCenter = center - t.position;
                var dist = toCenter.magnitude;
                if (dist < 0.0001f) continue;
                var dir = toCenter / dist;
                var falloff = 1f - Mathf.Clamp01(dist / _circularRadius);
                var step = _pullForce * falloff * Time.deltaTime;
                t.position += dir * step;
            }
        }
    }

    #region Anomally parameters

    [SerializeField] private float _pullForce = 1000f; // base pull strength
    [SerializeField] private float _circularRadius = 5f; // effect radius
    [SerializeField] private float _rotationForce; // tangential force to make objects orbit
    [SerializeField] private LayerMask _affectedLayers = ~0; // which layers are affected
    [SerializeField] private bool _includeKinematic; // affect kinematic rigidbodies
    [SerializeField] private bool _useFixedUpdate = true; // run in FixedUpdate for physics

    #endregion
}