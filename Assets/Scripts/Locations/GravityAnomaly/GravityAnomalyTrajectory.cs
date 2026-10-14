using UnityEngine;

/// <summary>
/// Holographic preview of where the ship will coast inside a gravity anomaly: the path for the next few seconds
/// with engines off, under the same pull and drag. It shows the consequence, not a route to follow,
/// so the player still finds the slingshot line on their own.
/// </summary>
[RequireComponent(typeof(GravityAnomaly))]
public class GravityAnomalyTrajectory : MonoBehaviour
{
    [SerializeField] private LineRenderer line;
    [SerializeField] private float previewSeconds = 4.5f;
    [SerializeField] private float stepSeconds = 0.1f;

    private GravityAnomaly anomaly;
    private Vector3[] points;

    private void Awake()
    {
        anomaly = GetComponent<GravityAnomaly>();
        points = new Vector3[Mathf.CeilToInt(previewSeconds / stepSeconds) + 1];
        if (line != null) line.enabled = false;
    }

    private void LateUpdate()
    {
        if (line == null) return;

        Player_Spaceship ship = anomaly.ShipInside;
        if (ship == null || !ship.TryGetComponent(out Rigidbody body))
        {
            line.enabled = false;
            return;
        }

        Vector3 position = body.position;
        Vector3 velocity = body.linearVelocity;
        float drag = body.linearDamping;
        int count = 0;
        points[count++] = position;

        while (count < points.Length)
        {
            velocity += anomaly.PullAt(position) * stepSeconds;
            velocity *= Mathf.Max(0f, 1f - drag * stepSeconds);
            position += velocity * stepSeconds;
            points[count++] = position;
            if (anomaly.IsInCore(position)) break;
        }

        line.positionCount = count;
        line.SetPositions(points);
        line.enabled = true;
    }
}
