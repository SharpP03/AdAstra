using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public enum PlayerState
{
    #region EDUCATION NOTE 
    // DO PRACY IN?. - przesuni?cia bitowem i zarz?dzanie maskami binarnie.
    // for more educational value and in case of nedd for complex system use bit operations and flags
    // e.g. 1<<1 1<<2 1<<3, bit shifting and marking as flags e.g. 1010  
    #endregion
    Standstill,
    Moving,
    Sprinting
}

[SelectionBase]
public class Player_Spaceship : MonoBehaviour
{
    #region Player Input system vars
    private PlayerInput playerInput;
    private InputAction moveAction;
    private InputAction rollAction;
    private InputAction mouseX;
    private InputAction mouseY;
    private InputAction sprintAction;
    #endregion

    #region movement multipliers
    [SerializeField] private bool realisticMovementOn = false;
    [SerializeField] private float moveSpeed = 1f;
    [SerializeField] private float sprintMultiplier = 1.5f;
    [SerializeField] private float accelerationRate = 5f; // Jak szybko zmienia si? pr?dko??
    [SerializeField] private float speedMultAngle = .5f;
    [SerializeField] private float speedRollAngle = .05f;
    [SerializeField] private float mouseSensX = 100f;
    [SerializeField] private float mouseSensY = 100f;
    #endregion

    private Rigidbody rb;
    private FuelSystem fuelSystem;
    private Transform cameraHolder;

    private PlayerState playerCurrentState = PlayerState.Standstill;

    private float currentSpeed;
    private float targetSpeed;

    private Vector2 bufferedMouseDelta;
    private float bufferedRollInput;
    private bool isSprintingInput;

    #region Input system lifecycle
    private void OnEnable()
    {
        moveAction?.Enable();
        rollAction?.Enable();
        mouseX?.Enable();
        mouseY?.Enable();
        sprintAction?.Enable();
    }

    private void OnDisable()
    {
        moveAction?.Disable();
        rollAction?.Disable();
        mouseX?.Disable();
        mouseY?.Disable();
        sprintAction?.Disable();
    }
    #endregion



    void Start()
    {
        SetInputs();
        rb = GetComponent<Rigidbody>();
        fuelSystem = GetComponent<FuelSystem>();
        GameManager.Instance.RegisterPlayer(this);

        currentSpeed = 0f;
        targetSpeed = moveSpeed;

        Cursor.lockState = CursorLockMode.Locked;

        cameraHolder = GetComponentsInChildren<Transform>(false)
            .FirstOrDefault(t => t.CompareTag("CameraAnchor"));
        if (cameraHolder == null)
            Debug.LogWarning("Camera Anchor not found!");
    }

    private void FixedUpdate()
    {
        HandlePlayerSpeed();
        if (fuelSystem.HasFuel)
            MovePlayer();
    }

    private void Update()
    {
        HandleInput();
        HandleState();

#if UNITY_EDITOR
        //if (Debug.isDebugBuild)
        //    Debug.Log($"Speed: {currentSpeed:F2}, State: {playerCurrentState}");
#endif
    }

    private void HandleInput()
    {
        MoveInput = Vector2.ClampMagnitude(moveAction.ReadValue<Vector2>(), 1f);
        bufferedRollInput = rollAction.ReadValue<float>();
        bufferedMouseDelta.x += mouseX.ReadValue<float>() * mouseSensX;
        bufferedMouseDelta.y += mouseY.ReadValue<float>() * mouseSensY;
        isSprintingInput = sprintAction.IsPressed();
    }

    private void HandlePlayerSpeed()
    {
        float baseSpeed = playerCurrentState switch
        {
            #region EDUCATION NOTE 
            // switch expression VS switch control flow (basic switch)
            // wyra?enie switch dost?pne od wersji C# 8.0 - nie pozwala na fallthrough, pokrywa stany, zwraca warto??
            #endregion
            PlayerState.Sprinting => moveSpeed * sprintMultiplier,
            PlayerState.Moving => moveSpeed,
            _ => 0f
        };

        // P?ynna zmiana pr?dko?ci
        targetSpeed = baseSpeed;
        currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, Time.fixedDeltaTime * accelerationRate);
    }

    private void HandleState()
    {
        Vector2 moveInput = MoveInput;
        bool isSprinting = isSprintingInput;
        bool isMoving = moveInput.sqrMagnitude > 0.0001f;

        // uwzgl?dnienie realistic mode
        bool isMovingHorizontal = Mathf.Abs(moveInput.y) > 0.0001f;

        if (realisticMovementOn)
        {
            if (isMovingHorizontal && isSprinting)
                playerCurrentState = PlayerState.Sprinting;
            else if (isMovingHorizontal)
                playerCurrentState = PlayerState.Moving;
            else
                playerCurrentState = PlayerState.Standstill;
        }
        else
        {
            if (isMoving && isSprinting)
                playerCurrentState = PlayerState.Sprinting;
            else if (isMoving)
                playerCurrentState = PlayerState.Moving;
            else
                playerCurrentState = PlayerState.Standstill;
        }
    }


    private void MovePlayer()
    {
        float rollInput = bufferedRollInput;
        Vector2 mouseDelta = bufferedMouseDelta;
        bufferedMouseDelta = Vector2.zero;

        // Ruch w jednej AddForce
        if (realisticMovementOn)
            MovePlayer_Realistic();
        else
            MovePlayer_Arcade();

        // Obr?t myszk?
        rb.AddTorque(rb.transform.right * -mouseDelta.y * speedMultAngle, ForceMode.Acceleration);
        rb.AddTorque(rb.transform.up * mouseDelta.x * speedMultAngle, ForceMode.Acceleration);

        // Roll
        rb.AddTorque(rb.transform.forward * rollInput * speedRollAngle, ForceMode.Acceleration);
    }

    private void MovePlayer_Arcade()
    {
        Vector2 moveInput = MoveInput;
        Vector3 moveDir = rb.transform.TransformDirection(new Vector3(moveInput.x, 0, moveInput.y));
        rb.AddForce(moveDir * currentSpeed, ForceMode.Acceleration);
    }

    private void MovePlayer_Realistic()
    {
        Vector2 moveInput = MoveInput;
        Vector3 moveDir = rb.transform.TransformDirection(new Vector3(0, 0, moveInput.y));
        rb.AddForce(moveDir * currentSpeed, ForceMode.Acceleration);
    }
    private void SetInputs()
    {
        playerInput = GetComponent<PlayerInput>();
        moveAction = playerInput.actions.FindAction("Move");
        rollAction = playerInput.actions.FindAction("Roll");
        mouseX = playerInput.actions.FindAction("MouseX");
        mouseY = playerInput.actions.FindAction("MouseY");
        sprintAction = playerInput.actions.FindAction("Sprint");
    }

    public Vector2 MoveInput { get; private set; }
    public PlayerState CurrentState => playerCurrentState;
    #region EDUCATION NOTE public getter
    // R?wnowa?ne z poni?szym zapisem
    //public PlayerState CurrentState
    //{
    //    get { return playerCurrentState; }
    //}
    #endregion

}
