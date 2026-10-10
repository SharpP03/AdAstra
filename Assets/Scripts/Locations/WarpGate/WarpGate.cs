using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Run goal at the end of the sector. Entering the capture zone locks the ship controls and pulls
/// the ship into the docking point, where the jump authorization (terminal) takes place.
/// </summary>
[SelectionBase]
[RequireComponent(typeof(Collider))]
public class WarpGate : MonoBehaviour
{
    [Header("Capture")]
    [Tooltip("Point in the middle of the gate where the captured ship is docked.")]
    [SerializeField] private Transform dockPoint;
    [Tooltip("Time in seconds to pull the ship from its capture position to the dock point.")]
    [SerializeField] private float captureDuration = 1f;

    [Header("Jump Authorization")]
    [SerializeField] private WarpGateWordPoolSO wordPool;

    [Header("Jump")]
    [Tooltip("Time in seconds the ship accelerates through the portal before the run ends.")]
    [SerializeField] private float jumpDuration = 1.6f;
    [Tooltip("Distance in meters the ship travels along the gate axis during the jump.")]
    [SerializeField] private float jumpDistance = 400f;

    public bool IsShipCaptured { get; private set; }
    public float JumpDuration => jumpDuration;

    /// <summary>Raised once the ship is docked and motionless inside the gate.</summary>
    public event Action<Player_Spaceship> OnShipDocked;

    /// <summary>Raised when the jump sequence has finished.</summary>
    public event Action OnJumpFinished;

    private void Awake()
    {
        GetComponent<Collider>().isTrigger = true;
        if (dockPoint == null) dockPoint = transform;
    }

    public string PickAuthorizationWord()
    {
        return wordPool != null ? wordPool.GetRandomWord() : "JUMP";
    }

    private void OnTriggerEnter(Collider other)
    {
        if (IsShipCaptured) return;
        if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameState.Playing) return;

        // Works with no fuel too: a drifting ship is captured like any other.
        var ship = other.GetComponentInParent<Player_Spaceship>();
        if (ship == null) return;

        StartCoroutine(CaptureRoutine(ship));
    }

    private IEnumerator CaptureRoutine(Player_Spaceship ship)
    {
        IsShipCaptured = true;
        ship.SetControlsEnabled(false);

        #region EDUCATION NOTE
        // DO PRACY INŻ. - przechwycenie statku: Rigidbody przechodzi w tryb kinematyczny, więc fizyka przestaje
        // na niego działać (pęd zostaje wygaszony), a pozycję i obrót prowadzi skrypt przez MovePosition/MoveRotation.
        // Krzywa ease-out (1 - (1 - t)^3) daje szybkie "złapanie" i miękkie wyhamowanie w punkcie dokowania.
        #endregion
        var rb = ship.GetComponent<Rigidbody>();
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic = true;

        Vector3 startPos = rb.position;
        Quaternion startRot = rb.rotation;
        float elapsed = 0f;

        while (elapsed < captureDuration)
        {
            elapsed += Time.fixedDeltaTime;
            float t = Mathf.Clamp01(elapsed / captureDuration);
            float eased = 1f - Mathf.Pow(1f - t, 3f);

            rb.MovePosition(Vector3.LerpUnclamped(startPos, dockPoint.position, eased));
            rb.MoveRotation(Quaternion.SlerpUnclamped(startRot, dockPoint.rotation, eased));
            yield return new WaitForFixedUpdate();
        }

        OnShipDocked?.Invoke(ship);
    }

    /// <summary>
    /// Pulls the docked ship through the portal with growing speed, then raises OnJumpFinished.
    /// </summary>
    public void Jump(Player_Spaceship ship)
    {
        StartCoroutine(JumpRoutine(ship.GetComponent<Rigidbody>()));
    }

    private IEnumerator JumpRoutine(Rigidbody rb)
    {
        Vector3 startPos = rb.position;
        Vector3 axis = dockPoint.forward;
        float elapsed = 0f;

        while (elapsed < jumpDuration)
        {
            elapsed += Time.fixedDeltaTime;
            float t = Mathf.Clamp01(elapsed / jumpDuration);
            rb.MovePosition(startPos + axis * (jumpDistance * t * t * t)); // ease-in: slow start, violent exit
            yield return new WaitForFixedUpdate();
        }

        OnJumpFinished?.Invoke();
    }
}
