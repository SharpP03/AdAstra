using UnityEngine;

public class DynamicCamera : MonoBehaviour
{
    [Header("Target to follow")]
    [SerializeField] private Transform target;

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
    [SerializeField] private float rollFollowStrength = 0.5f; // Jak mocno kamera pod¹¿a za roll'em statku (0–1)

    private Player_Spaceship player;
    private Vector3 currentVelocity;
    private Vector3 currentOffset;
    private Camera cam;
    private float baseFOV;

    private float currentRoll; // przechowywany roll kamery

    private void Start()
    {
        if (target == null)
        {
            Debug.LogError("DynamicCamera: No target assigned!");
            enabled = false;
            return;
        }

        //player = target.GetComponent<Player_Spaceship>();
        player = GameManager.Instance.Player;

        cam = GetComponent<Camera>();
        if (cam) baseFOV = cam.fieldOfView;

        currentOffset = baseOffset;
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

        // odczyt roll'a statku (obrót wokó³ osi Z)
        #region NOTA EDUKACYJNA obrót skoki
        // taka kalkulacjia powodowa³a nag³e skoki kamery przy pe³nym obrocie 
        //float shipRoll = target.eulerAngles.z;
        //if (shipRoll > 180) shipRoll -= 360; // normalizacja zakresu [-180, 180]
        //currentRoll = Mathf.Lerp(currentRoll, shipRoll * rollFollowStrength, Time.deltaTime * tiltSmoothness);
        #endregion
        // p³ynne przejœcie 
        float targetRoll = target.eulerAngles.z;
        float smoothRoll = Mathf.DeltaAngle(currentRoll / rollFollowStrength, targetRoll) * rollFollowStrength;
        currentRoll += smoothRoll * Time.deltaTime * tiltSmoothness;


        // tworzymy finaln¹ rotacjê kamery (forward statku + tilt + roll)
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
