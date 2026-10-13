using System;
using UnityEngine;

/// <summary>
/// Transit wreck: a stationary hulk with a blinking beacon and cargo crates slowly orbiting it.
/// Crates are kinematic, so an impact hurts the ship but never knocks a crate off its orbit.
/// Each crate orbits around its own axis from the position it was placed at in the prefab.
/// </summary>
[SelectionBase]
public class SalvageWreck : MonoBehaviour
{
    [Serializable]
    private class CrateOrbit
    {
        public SalvageCrate crate;
        [Tooltip("Orbit axis in the wreck's local space.")]
        public Vector3 axis = Vector3.up;
        [Tooltip("Degrees per second around the wreck. Negative reverses the direction.")]
        public float angularSpeed = 8f;
        [Tooltip("Degrees per second the crate tumbles around itself.")]
        public float tumbleSpeed = 20f;

        [NonSerialized] public Rigidbody body;
        [NonSerialized] public Vector3 offset;
        [NonSerialized] public Vector3 tumbleAxis;
        [NonSerialized] public float angle;
    }

    [SerializeField] private CrateOrbit[] orbits = Array.Empty<CrateOrbit>();

    [Header("Beacon")]
    [SerializeField] private Light beaconLight;
    [Tooltip("Glowing visual that blinks together with the light.")]
    [SerializeField] private Renderer beaconVisual;
    [SerializeField] private float blinkInterval = 1.2f;
    [SerializeField] private float blinkOnTime = 0.25f;

    private void Start()
    {
        foreach (CrateOrbit orbit in orbits)
        {
            if (orbit.crate == null) continue;
            orbit.body = orbit.crate.GetComponent<Rigidbody>();
            orbit.offset = orbit.crate.transform.position - transform.position;
            Vector3 cross = Vector3.Cross(orbit.axis, orbit.offset);
            orbit.tumbleAxis = cross.sqrMagnitude > 0.0001f ? cross.normalized : Vector3.right;
        }
    }

    private void FixedUpdate()
    {
        foreach (CrateOrbit orbit in orbits)
        {
            // An opened crate is deactivated; its orbit stops with it.
            if (orbit.body == null || !orbit.crate.gameObject.activeInHierarchy) continue;

            orbit.angle += orbit.angularSpeed * Time.fixedDeltaTime;
            Quaternion around = Quaternion.AngleAxis(orbit.angle, transform.TransformDirection(orbit.axis));
            orbit.body.MovePosition(transform.position + around * orbit.offset);
            orbit.body.MoveRotation(Quaternion.AngleAxis(orbit.tumbleSpeed * Time.fixedDeltaTime, orbit.tumbleAxis) * orbit.body.rotation);
        }
    }

    private void Update()
    {
        bool on = Time.time % blinkInterval < blinkOnTime;
        if (beaconLight != null) beaconLight.enabled = on;
        if (beaconVisual != null) beaconVisual.enabled = on;
    }
}
