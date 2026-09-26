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

    [Header("Tilt and roll following")]
    [SerializeField] private float tiltAmount = 5f;
    [SerializeField] private float tiltSmoothness = 5f;
    [SerializeField] private float rollFollowStrength = 0.5f; // Jak mocno kamera podąża za roll'em statku (0-1)

    private Transform target;
    private Vector3 currentVelocity;
    private Vector3 currentOffset;
    private Camera cam;
    private float baseFOV;

    private float currentRoll; // przechowywany roll kamery

    private void Awake()
    {
        ResolveTarget();
    }

    private void Start()
    {
        if (player == null)
        {
            Debug.LogError("DynamicCamera: No Player_Spaceship assigned!");
            enabled = false;
            return;
        }

        cam = GetComponent<Camera>();
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

        bool isSprinting = player.CurrentState == PlayerState.Sprinting;

        Vector3 targetOffset = baseOffset + (isSprinting ? new Vector3(0, 0, -sprintZoomOut) : Vector3.zero);
        currentOffset = Vector3.Lerp(currentOffset, targetOffset, Time.deltaTime * sprintTransitionSpeed);

        float targetFOV = isSprinting ? baseFOV + sprintFOVIncrease : baseFOV;
        cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, targetFOV, Time.deltaTime * sprintTransitionSpeed);
    }
}