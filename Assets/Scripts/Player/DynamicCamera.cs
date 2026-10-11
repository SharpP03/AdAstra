using UnityEngine;

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

    [Header("Warp jump effect")]
    [SerializeField] private float warpFOVIncrease = 45f;

    [Header("Tilt and roll following")]
    [SerializeField] private float tiltAmount = 5f;
    [SerializeField] private float tiltSmoothness = 5f;
    [SerializeField] private float rollFollowStrength = 0.5f; // Jak mocno kamera podąża za roll'em statku (0-1)

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

    private float currentRoll; // przechowywany roll kamery
    private float warpDuration;
    private float warpElapsed;
    private float trauma;

    private void Awake()
    {
        ResolveTarget();
    }

    private void OnEnable()
    {
        if (hullImpacts != null) hullImpacts.OnImpact += HandleImpact;
    }

    private void OnDisable()
    {
        if (hullImpacts != null) hullImpacts.OnImpact -= HandleImpact;
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
    }

    private void HandleImpact(float strength, Vector3 point, Vector3 normal)
    {
        // Hits add up instead of restarting the shake.
        trauma = Mathf.Clamp01(trauma + Mathf.Lerp(minImpactTrauma, 1f, strength));
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
        float shake = trauma * trauma;
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
        Vector2 moveInput = player.MoveInput;
        float tiltZ = -moveInput.x * tiltAmount;
        float tiltX = moveInput.y * tiltAmount * 0.3f;

        // odczyt roll'a statku (obrót wokół osi Z)
        #region NOTA EDUKACYJNA obrót skoki
        // taka kalkulacja powodowała nagłe skoki kamery przy pełnym obrocie 
        //float shipRoll = target.eulerAngles.z;
        //if (shipRoll > 180) shipRoll -= 360; // normalizacja zakresu [-180, 180]
        //currentRoll = Mathf.Lerp(currentRoll, shipRoll * rollFollowStrength, Time.deltaTime * tiltSmoothness);
        #endregion
        // płynne przejście 
        float targetRoll = target.eulerAngles.z;
        float smoothRoll = Mathf.DeltaAngle(currentRoll / rollFollowStrength, targetRoll) * rollFollowStrength;
        currentRoll += smoothRoll * Time.deltaTime * tiltSmoothness;


        // tworzymy finalną rotację kamery (forward statku + tilt + roll)
        Quaternion baseRot = Quaternion.LookRotation(target.forward, Vector3.up);
        Quaternion tiltRot = Quaternion.Euler(tiltX, 0, tiltZ + currentRoll);
        Quaternion finalRot = baseRot * tiltRot;

        transform.rotation = Quaternion.Slerp(transform.rotation, finalRot, rotationSmoothness * Time.deltaTime);
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

        bool isSprinting = player.CurrentState == PlayerState.Sprinting;

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