using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public enum PlayerState
{
    #region EDUCATION NOTE 
    // DO PRACY IN¯. - przesuniêcia bitowem i zarz¹dzanie maskami binarnie.
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
    [SerializeField] private float moveSpeed = 1f;
    [SerializeField] private float sprintMultiplier = 1.5f;
    [SerializeField] private float accelerationRate = 5f; // Jak szybko zmienia siê prêdkoœæ
    [SerializeField] private float speedMultAngle = .5f;
    [SerializeField] private float speedRollAngle = .05f;
    [SerializeField] private float mouseSensX = 100f;
    [SerializeField] private float mouseSensY = 100f;
    #endregion

    [SerializeField] private Transform mainCamera;
    private Rigidbody rb;
    private FuelSystem fuelSystem;
    private Transform cameraHolder;

    private PlayerState playerCurrentState = PlayerState.Standstill;

    private float currentSpeed;
    private float targetSpeed;

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
        HandleState();

#if UNITY_EDITOR
        if (Debug.isDebugBuild)
            Debug.Log($"Speed: {currentSpeed:F2}, State: {playerCurrentState}");
#endif
    }

    private void HandlePlayerSpeed()
    {
        float baseSpeed = playerCurrentState switch
        {
            #region EDUCATION NOTE 
            // switch expression VS switch control flow (basic switch)
            // wyra¿enie switch dostêpne od wersji C# 8.0 - nie pozwala na fallthrough, pokrywa stany, zwraca wartoœæ
            #endregion
            PlayerState.Sprinting => moveSpeed * sprintMultiplier,
            PlayerState.Moving => moveSpeed,
            _ => 0f
        };

        // P³ynna zmiana prêdkoœci
        targetSpeed = baseSpeed;
        currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, Time.deltaTime * accelerationRate);
    }

    private void HandleState()
    {
        Vector2 moveInput = moveAction.ReadValue<Vector2>();
        bool isSprinting = sprintAction.IsPressed();

        if (moveInput.magnitude == 0)
            playerCurrentState = PlayerState.Standstill;
        else if (isSprinting)
            playerCurrentState = PlayerState.Sprinting;
        else
            playerCurrentState = PlayerState.Moving;
    }

    private void MovePlayer()
    {
        Vector2 moveInput = moveAction.ReadValue<Vector2>();
        float rollInput = rollAction.ReadValue<float>();
        float mouseInputX = mouseX.ReadValue<float>() * Time.deltaTime * mouseSensX;
        float mouseInputY = mouseY.ReadValue<float>() * Time.deltaTime * mouseSensY;

        // Ruch w jednej AddForce
        Vector3 moveDirection = rb.transform.TransformDirection(new Vector3(moveInput.x, 0, moveInput.y));
        rb.AddForce(moveDirection * currentSpeed, ForceMode.Acceleration);

        // Obrót myszk¹
        rb.AddTorque(rb.transform.right * -mouseInputY * speedMultAngle, ForceMode.Acceleration);
        rb.AddTorque(rb.transform.up * mouseInputX * speedMultAngle, ForceMode.Acceleration);

        // Roll
        rb.AddTorque(rb.transform.forward * rollInput * speedRollAngle, ForceMode.Acceleration);
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
    public PlayerState CurrentState => playerCurrentState;
    #region EDUCATION NOTE public getter
    // Równowa¿ne z poni¿szym zapisem
    //public PlayerState CurrentState
    //{
    //    get { return playerCurrentState; }
    //}
    #endregion

}
