using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class DynamicCamera : MonoBehaviour
{
    [Header("Target to follow")]
    [SerializeField] private Player_Spaceship player;
    [Tooltip("Optional custom anchor to follow. If null, player.transform will be used.")]
    [SerializeField] private Transform followAnchor;

    [Header("Camera positioning")]
    [SerializeField] private Vector3 baseOffset = new Vector3(0, 2, -6);
    [SerializeField] private float followSmoothness = 5f;
    [SerializeField] private float rotationSmoothness = 5f;

    [Header("Sprint effect")]
    [SerializeField] private float sprintZoomOut = 2f;
    [SerializeField] private float sprintFOVIncrease = 15f;
    [SerializeField] private float sprintTransitionSpeed = 3f;

    [Header("Slingshot effect")]
    [Tooltip("Instant FOV jump when a gravity anomaly launches the ship, at full strength.")]
    [SerializeField] private float slingshotFOVKick = 25f;
    [SerializeField] private float slingshotTrauma = 0.5f;

    [Header("Gravity anomaly depth")]
    [Tooltip("Continuous shake at the core border of a gravity anomaly (same scale as trauma²).")]
    [SerializeField] private float gravityRumble = 0.15f;
    [Tooltip("Volume whose vignette darkens the screen edges deep in a gravity anomaly (its profile is copied at runtime).")]
    [SerializeField] private Volume postVolume;
    [SerializeField] private float gravityVignette = 0.55f;

    [Header("Warp jump effect")]
    [SerializeField] private float warpFOVIncrease = 45f;

    [Header("Impact shake")]
    [SerializeField] private HullCollisionDamage hullImpacts;
    [Tooltip("Trauma added by the weakest damaging impact; the strongest adds 1.")]
    [SerializeField] private float minImpactTrauma = 0.25f;
    [SerializeField] private float traumaDecay = 1.5f; // trauma lost per second
    [SerializeField] private float maxShakeAngle = 4f; // degrees
    [SerializeField] private float maxShakeOffset = 0.3f;
    [SerializeField] private float shakeFrequency = 25f;

    private Transform target;
    private Vector3 currentVelocity;
    private Vector3 currentOffset;
    private Camera cam;
    private float baseFOV;

    private float warpDuration;
    private float warpElapsed;
    private float trauma;
    private Vignette vignette;
    private bool slingshotLook;
    private float baseVignette;

    private void Awake()
    {
        ResolveTarget();
    }

    private void OnEnable()
    {
        if (hullImpacts != null) hullImpacts.OnImpact += HandleImpact;
        if (player != null) player.OnSlingshot += HandleSlingshot;
    }

    private void OnDisable()
    {
        if (hullImpacts != null) hullImpacts.OnImpact -= HandleImpact;
        if (player != null) player.OnSlingshot -= HandleSlingshot;
    }

    private void Start()
    {
        if (player == null)
        {
            Debug.LogError("DynamicCamera: No Player_Spaceship assigned!");
            enabled = false;
            return;
        }

        cam = GetComponentInChildren<Camera>(); // the Camera sits on a child of the rig
        if (cam) baseFOV = cam.fieldOfView;

        currentOffset = baseOffset;

        if (postVolume != null)
        {
            // Reading .profile gives a runtime copy, so the profile asset is never changed in the editor.
            VolumeProfile profile = postVolume.profile;
            if (!profile.TryGet(out vignette)) vignette = profile.Add<Vignette>(true);
            baseVignette = vignette.intensity.overrideState ? vignette.intensity.value : 0f;
            vignette.intensity.overrideState = true;
        }
    }

    private void OnValidate()
    {
        ResolveTarget();
    }

    private void ResolveTarget()
    {
        if (followAnchor != null)
        {
            target = followAnchor;
        }
        else if (player != null)
        {
            target = player.transform;
        }
    }

    private void LateUpdate()
    {
        if (player == null || target == null) return;

        UpdateCameraPosition();
        UpdateCameraRotation();
        UpdateSprintEffects();
        UpdateImpactShake();
        UpdateGravityVignette();
    }

    private void UpdateGravityVignette()
    {
        if (vignette == null) return;
        vignette.intensity.value = Mathf.Lerp(baseVignette, gravityVignette, player.GravityStress);
    }

    private void HandleImpact(float strength, Vector3 point, Vector3 normal)
    {
        // Hits add up instead of restarting the shake.
        trauma = Mathf.Clamp01(trauma + Mathf.Lerp(minImpactTrauma, 1f, strength));
    }

    private void HandleSlingshot(float strength)
    {
        // Kick the FOV at once; the boost look below then holds it until the slingshot ends.
        if (cam) cam.fieldOfView = Mathf.Max(cam.fieldOfView, baseFOV + sprintFOVIncrease + slingshotFOVKick * strength);
        trauma = Mathf.Clamp01(trauma + slingshotTrauma * strength);
        slingshotLook = true;
    }

    private void UpdateImpactShake()
    {
        if (!cam) return;

        #region EDUCATION NOTE impact shake
        // DO PRACY INŻ. - wstrząs kamery oparty na "traumie" (trauma-based screen shake).
        // Trauma 0..1 rośnie przy uderzeniu i liniowo wygasa; siła wstrząsu to trauma^2,
        // więc lekkie otarcia ledwo poruszają obrazem, a mocne uderzenia są wyraźne.
        // Szum Perlina zamiast Random daje płynny ruch zamiast drgań "szumu".
        // Wstrząs przesuwa tylko kamerę-dziecko rigu, więc nie zaburza wygładzania podążania
        // ani fizyki statku.
        #endregion
        trauma = Mathf.Max(0f, trauma - traumaDecay * Time.deltaTime);
        // Deep in a gravity anomaly the camera rumbles steadily; hits on top of it still shake harder.
        float shake = Mathf.Max(trauma * trauma, player.GravityStress * gravityRumble);
        float t = Time.time * shakeFrequency;

        cam.transform.localRotation = Quaternion.Euler(
            maxShakeAngle * shake * (Mathf.PerlinNoise(t, 0f) * 2f - 1f),
            maxShakeAngle * shake * (Mathf.PerlinNoise(0f, t) * 2f - 1f),
            maxShakeAngle * shake * (Mathf.PerlinNoise(t, t) * 2f - 1f));
        cam.transform.localPosition = maxShakeOffset * shake * new Vector3(
            Mathf.PerlinNoise(t + 10f, 0f) * 2f - 1f,
            Mathf.PerlinNoise(0f, t + 10f) * 2f - 1f,
            0f);
    }

    private void UpdateCameraPosition()
    {
        Vector3 desiredPosition = target.TransformPoint(currentOffset);
        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref currentVelocity, 1f / followSmoothness);
    }

    private void UpdateCameraRotation()
    {
        #region NOTA EDUKACYJNA obrót skoki
        // DO PRACY INŻ. - kamera podąża za pełną rotacją statku (kwaternion), a nie za kątami Eulera.
        // Wcześniej roll czytany z target.eulerAngles.z i LookRotation(forward, Vector3.up) powodowały
        // nagłe skoki przy pionowym locie (rozkład na kąty Eulera przeskakuje przy pitch ±90°).
        // Usunięto też przechył zależny od surowego wejścia (A/D, W/S): cel zmieniał się skokowo
        // przy wciśnięciu klawisza, a w trybie realistycznym kamera przechylała się, choć statek nie ruszał.
        // Wygładzanie 1 - e^(-k·dt) daje ten sam efekt niezależnie od liczby klatek.
        #endregion
        float t = 1f - Mathf.Exp(-rotationSmoothness * Time.deltaTime);
        transform.rotation = Quaternion.Slerp(transform.rotation, target.rotation, t);
    }

    private void UpdateSprintEffects()
    {
        if (!cam) return;

        if (warpDuration > 0f)
        {
            // Warp jump: FOV widens with ease-in, overriding the sprint effect until the run ends.
            warpElapsed += Time.deltaTime;
            float t = Mathf.Clamp01(warpElapsed / warpDuration);
            cam.fieldOfView = baseFOV + warpFOVIncrease * t * t;
            return;
        }

        // A fast slingshot exit looks like a boost for as long as its low drag lasts.
        if (!player.InSlingshot) slingshotLook = false;
        bool isSprinting = player.CurrentState == PlayerState.Sprinting || slingshotLook;

        Vector3 targetOffset = baseOffset + (isSprinting ? new Vector3(0, 0, -sprintZoomOut) : Vector3.zero);
        currentOffset = Vector3.Lerp(currentOffset, targetOffset, Time.deltaTime * sprintTransitionSpeed);

        float targetFOV = isSprinting ? baseFOV + sprintFOVIncrease : baseFOV;
        cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, targetFOV, Time.deltaTime * sprintTransitionSpeed);
    }

    /// <summary>
    /// Starts the Warp Gate jump camera effect: a FOV kick that builds up over the given duration.
    /// </summary>
    public void PlayWarpEffect(float duration)
    {
        warpDuration = Mathf.Max(0.01f, duration);
        warpElapsed = 0f;
    }
}