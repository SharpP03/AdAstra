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

    [SerializeField] private bool realisticMovementOn = false;
    [Tooltip("Thrust, drag, rotation and fuel balance values.")]
    [SerializeField] private ShipFlightConfigSO config;

    private Rigidbody rb;
    private FuelSystem fuelSystem;
    private Transform cameraHolder;

    private PlayerState playerCurrentState = PlayerState.Standstill;

    private float currentSpeed;
    private float targetSpeed;

    private Vector2 bufferedMouseDelta;
    private float bufferedRollInput;
    private bool isSprintingInput;
    private bool controlsEnabled = true;
    private float slingshotEndTime;

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

        if (config == null)
        {
            Debug.LogError("Player_Spaceship: missing ShipFlightConfigSO.", this);
            enabled = false;
            return;
        }

        currentSpeed = 0f;
        targetSpeed = config.thrustAcceleration;

        Cursor.lockState = CursorLockMode.Locked;

        cameraHolder = GetComponentsInChildren<Transform>(false)
            .FirstOrDefault(t => t.CompareTag("CameraAnchor"));
        if (cameraHolder == null)
            Debug.LogWarning("Camera Anchor not found!");
    }

    private void FixedUpdate()
    {
        #region EDUCATION NOTE dryf bez paliwa
        // DO PRACY INŻ. - dwa poziomy oporu liniowego. Przy działających silnikach opór ogranicza prędkość
        // maksymalną (ok. ciąg / opór), więc gracz nie przyspiesza w nieskończoność. Po wyczerpaniu paliwa opór
        // spada prawie do zera: statek dryfuje na resztce pędu (droga ok. v / opór) i dopiero powoli staje.
        // Ostatnie paliwo można więc spalić na boost i "wystrzelić się" np. w stronę stacji.
        // Niski opór tylko przy pustym baku, bo przy niskim oporze zawsze opłacałoby się "pyknąć" ciąg i lecieć
        // za darmo – paliwo przestałoby być budżetem dystansu.
        // Wyjątek: wyrzut z anomalii grawitacyjnej (Slingshot) na kilka sekund obniża opór do poziomu pustego baku,
        // bo przy zwykłym oporze dodana prędkość wygasłaby po ok. 2 s i nie dałaby odczuwalnego dystansu.
        #endregion
        // Applied every step so drag can be tuned on the config asset during Play Mode.
        rb.linearDamping = fuelSystem.HasFuel && !InSlingshot ? config.linearDrag : config.emptyTankDrag;
        rb.angularDamping = config.angularDrag;

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
        if (!controlsEnabled)
        {
            MoveInput = Vector2.zero;
            bufferedRollInput = 0f;
            bufferedMouseDelta = Vector2.zero;
            isSprintingInput = false;
            return;
        }

        MoveInput = Vector2.ClampMagnitude(moveAction.ReadValue<Vector2>(), 1f);
        bufferedRollInput = rollAction.ReadValue<float>();
        bufferedMouseDelta.x += mouseX.ReadValue<float>() * config.mouseSensitivity;
        bufferedMouseDelta.y += mouseY.ReadValue<float>() * config.mouseSensitivity;
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
            PlayerState.Sprinting => config.thrustAcceleration * config.boostMultiplier,
            PlayerState.Moving => config.thrustAcceleration,
            _ => 0f
        };

        // P?ynna zmiana pr?dko?ci
        targetSpeed = baseSpeed;
        currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, Time.fixedDeltaTime * config.throttleResponse);
    }

    private void HandleState()
    {
        // Dead engines: no thrust state, so fuel, thruster FX and the boost camera all see an idle ship.
        if (!fuelSystem.HasFuel)
        {
            playerCurrentState = PlayerState.Standstill;
            return;
        }

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
        rb.AddTorque(rb.transform.right * -mouseDelta.y * config.pitchYawTorque, ForceMode.Acceleration);
        rb.AddTorque(rb.transform.up * mouseDelta.x * config.pitchYawTorque, ForceMode.Acceleration);

        // Roll
        rb.AddTorque(rb.transform.forward * rollInput * config.rollTorque, ForceMode.Acceleration);
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

    /// <summary>
    /// Locks or unlocks pilot input (end of run, Warp Gate capture). The ship keeps its momentum.
    /// </summary>
    public void SetControlsEnabled(bool enabled)
    {
        controlsEnabled = enabled;
    }

    /// <summary>False while pilot input is locked (end of run, Warp Gate capture).</summary>
    public bool ControlsEnabled => controlsEnabled;

    /// <summary>Raised by Slingshot; the argument is the launch strength from 0 to 1.</summary>
    public event System.Action<float> OnSlingshot;

    /// <summary>True while a slingshot keeps drag at the empty-tank level.</summary>
    public bool InSlingshot => Time.time < slingshotEndTime;

    /// <summary>
    /// Launches the ship along its flight direction (gravity anomaly exit): adds speed and keeps drag
    /// at the empty-tank level for a while, so the extra speed turns into distance instead of fading at once.
    /// </summary>
    public void Slingshot(float speedGain, float lowDragSeconds, float strength)
    {
        Vector3 direction = rb.linearVelocity.sqrMagnitude > 0.01f ? rb.linearVelocity.normalized : transform.forward;
        rb.AddForce(direction * speedGain, ForceMode.VelocityChange);
        slingshotEndTime = Time.time + lowDragSeconds;
        OnSlingshot?.Invoke(strength);
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
