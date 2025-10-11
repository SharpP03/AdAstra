using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

[SelectionBase]
public class Player_Spaceship : MonoBehaviour
{


    #region Player Input system vars
    PlayerInput playerInput;
    InputAction moveAction;
    InputAction rollAction;
    InputAction mouseX;
    InputAction mouseY;
    #endregion

    #region movement multipliers
    [SerializeField]
    private float speedMult = 1f;
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

    #region Input Sysytem related functions
    private void OnEnable()
    {
        moveAction.Enable();
        rollAction.Enable();
        mouseX.Enable();
        mouseY.Enable();
    }

    private void OnDisable()
    {
        moveAction.Disable();
        rollAction.Disable();
        mouseX.Disable();
        mouseY.Disable();
    }
    #endregion

    private void Awake()
    {
        SetInputs();

        rb = GetComponent<Rigidbody>();
        fuelSystem = GetComponent<FuelSystem>();

        GameManager.Instance.RegisterPlayer(this);
        fuelSystem.PlayerMovementInit(moveAction);
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;

        CameraHolder = GetComponentsInChildren<Transform>(false)
                .FirstOrDefault(t => t.CompareTag("CameraAnchor"));
        if (CameraHolder == null) { Debug.LogWarning("Camera Anchor not found !"); }
    }

    void Update()
    {

    }

    private void FixedUpdate()
    {
        if (fuelSystem.HasFuel)
        {
            MovePlayer();
        }
    }  

    private void MovePlayer()
    {
        float mouseInputX = mouseX.ReadValue<float>() * Time.deltaTime * mouseSensX;
        float mouseInputY = mouseY.ReadValue<float>() * Time.deltaTime * mouseSensY;

        Vector2 moveInput = moveAction.ReadValue<Vector2>();
        float rollInput = rollAction.ReadValue<float>();

        rb.AddForce(rb.transform.TransformDirection(Vector3.forward)*moveInput.y*speedMult,ForceMode.VelocityChange);
        rb.AddForce(rb.transform.TransformDirection(Vector3.right)*moveInput.x*speedMult,ForceMode.VelocityChange);

        rb.AddTorque(rb.transform.right * mouseInputY * speedMultAngle * -1,ForceMode.VelocityChange);
        rb.AddTorque(rb.transform.up * mouseInputX * speedMultAngle, ForceMode.VelocityChange);


        rb.AddTorque(rb.transform.forward * rollInput * speedRollAngle,ForceMode.VelocityChange);

    }

    private void LookAround() { 
    
    }

    private void SetInputs() {
        playerInput = GetComponent<PlayerInput>();
        moveAction = playerInput.actions.FindAction("Move");
        rollAction = playerInput.actions.FindAction("Roll");
        mouseX = playerInput.actions.FindAction("MouseX");
        mouseY = playerInput.actions.FindAction("MouseY");

    }

    private Transform CameraHolder;

}
