using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

[SelectionBase]
public class Player_Spaceship : MonoBehaviour
{

    private Rigidbody rb;

    PlayerInput playerInput;
    InputAction moveAction;
    InputAction rollAction;
    InputAction lookAction;

    [SerializeField]
    private float speedMult = 1f;
    [SerializeField]
    private float speedMultAngle = .05f;
    [SerializeField]
    private float speedRollAngle = .05f;
    [SerializeField]
    private float mouseSensitivity = 100f;
    [SerializeField]
    private Transform mainCamera;

    #region Input Sysytem related functions
    private void OnEnable()
    {
        moveAction.Enable();
        rollAction.Enable();
        lookAction.Enable();
    }

    private void OnDisable()
    {
        moveAction.Disable();
        rollAction.Disable();
        lookAction.Disable();
    }
    #endregion

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        playerInput = GetComponent<PlayerInput>();
        moveAction = playerInput.actions.FindAction("Move");
        rollAction = playerInput.actions.FindAction("Roll");
        lookAction = playerInput.actions.FindAction("Look");

        CameraHolder = GetComponentsInChildren<Transform>(false)
                .FirstOrDefault(t => t.CompareTag("CameraAnchor"));
        if (CameraHolder == null) { Debug.LogWarning("Camera Anchor not found !"); }
    }

    void Update()
    {
        //CameraFollow();
    }

    private void FixedUpdate()
    {
        MovePlayer();
    }

    void MovePlayer()
    {
        Vector2 mouseInput = lookAction.ReadValue<Vector2>() * mouseSensitivity * Time.fixedDeltaTime;
        Vector2 moveInput = moveAction.ReadValue<Vector2>();
        float rollInput = rollAction.ReadValue<float>();



        Debug.Log("x: " + mouseInput.x + " y: " + mouseInput.y);
        rb.AddForce(rb.transform.TransformDirection(Vector3.forward)*moveInput.y*speedMult,ForceMode.VelocityChange);
        rb.AddForce(rb.transform.TransformDirection(Vector3.right)*moveInput.x*speedMult,ForceMode.VelocityChange);

        rb.AddTorque(rb.transform.right * mouseInput.y * speedMultAngle * -1,ForceMode.VelocityChange);
        rb.AddTorque(rb.transform.up * mouseInput.x * speedMultAngle, ForceMode.VelocityChange);

        //rb.AddTorque(rb.transform.forward * rollInput * speedMultAngle,ForceMode.VelocityChange);
        
    }

    private Transform CameraHolder;
    void CameraFollow() { 
        mainCamera.transform.position =  CameraHolder.position;
        mainCamera.transform.rotation = CameraHolder.rotation;
    }
}
