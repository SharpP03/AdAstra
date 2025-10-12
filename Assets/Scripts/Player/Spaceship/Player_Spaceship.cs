using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Playables;

public enum PlayerState
{
    #region NOTE for improvement (educations)
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
    PlayerInput playerInput;
    InputAction moveAction;
    InputAction rollAction;
    InputAction mouseX;
    InputAction mouseY;
    InputAction sprintAction;
    #endregion

    #region movement multipliers
    [SerializeField]
    private float moveSpeed = 1f;
    private float currentSpeed;
    private float sprintSpeed;
    [SerializeField]
    private float speedMultAngle = .5f;
    [SerializeField]
    private float speedRollAngle = .05f;
    [SerializeField]
    private float mouseSensX = 100f;
    [SerializeField]
    private float mouseSensY = 100f;
    #endregion

    [SerializeField]
    private Transform mainCamera;
    private Rigidbody rb;

    private FuelSystem fuelSystem;
    private Transform CameraHolder;

    private PlayerState playerCurrentState = PlayerState.Standstill;

    #region Input Sysytem related functions
    private void OnEnable()
    {
        //moveAction.Enable();
        //rollAction.Enable();
        //mouseX.Enable();
        //mouseY.Enable();
        //sprintAction.Enable();
    }

    private void OnDisable()
    {
        moveAction.Disable();
        rollAction.Disable();
        mouseX.Disable();
        mouseY.Disable();
        sprintAction.Disable();
    }
    #endregion

    private void Awake()
    {

    }

    void Start()
    {
        SetInputs();

        rb = GetComponent<Rigidbody>();
        fuelSystem = GetComponent<FuelSystem>();
        GameManager.Instance.RegisterPlayer(this);
        currentSpeed = moveSpeed;
        sprintSpeed = moveSpeed*1.5f;

        Cursor.lockState = CursorLockMode.Locked;

        CameraHolder = GetComponentsInChildren<Transform>(false)
                .FirstOrDefault(t => t.CompareTag("CameraAnchor"));
        if (CameraHolder == null) { Debug.LogWarning("Camera Anchor not found !"); }
    }


    private void FixedUpdate()
    {
        if (fuelSystem.HasFuel)
        { 
            MovePlayer();
        }
    }

    private void Update()
    {
        HandleState();
        HandlePlayerSpeed();
        fuelSystem.UpdatePlayerState(playerCurrentState);
    }

    private void HandlePlayerSpeed() {
        switch (playerCurrentState)
        {
            case PlayerState.Moving or PlayerState.Standstill:
                currentSpeed = moveSpeed;
                break;
            case PlayerState.Sprinting:
                currentSpeed = sprintSpeed;
                break;
            default: Debug.LogWarning("Unhandled state: " + playerCurrentState); break;
        }
        Debug.Log("current speed: "+currentSpeed+",  state: "+playerCurrentState);
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
        float mouseInputX = mouseX.ReadValue<float>() * Time.deltaTime * mouseSensX;
        float mouseInputY = mouseY.ReadValue<float>() * Time.deltaTime * mouseSensY;

        Vector2 moveInput = moveAction.ReadValue<Vector2>();
        float rollInput = rollAction.ReadValue<float>();

        rb.AddForce(rb.transform.TransformDirection(Vector3.forward)*moveInput.y*currentSpeed,ForceMode.VelocityChange);
        rb.AddForce(rb.transform.TransformDirection(Vector3.right)*moveInput.x*currentSpeed,ForceMode.VelocityChange);

        rb.AddTorque(rb.transform.right * mouseInputY * speedMultAngle * -1,ForceMode.VelocityChange);
        rb.AddTorque(rb.transform.up * mouseInputX * speedMultAngle, ForceMode.VelocityChange);


        rb.AddTorque(rb.transform.forward * rollInput * speedRollAngle,ForceMode.VelocityChange);

    }

    private void SetInputs() {
        playerInput = GetComponent<PlayerInput>();
        moveAction = playerInput.actions.FindAction("Move");
        rollAction = playerInput.actions.FindAction("Roll");
        mouseX = playerInput.actions.FindAction("MouseX");
        mouseY = playerInput.actions.FindAction("MouseY");
        sprintAction = playerInput.actions.FindAction("Sprint");

    }


}
