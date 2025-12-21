using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class BeeFlight : MonoBehaviour
{
    [Header("Input")]
    public InputAction Look;      
    public InputAction Move;      
    public InputAction RollInput; 

    [Header("Flight")]
    public float flySpeed = 7f;
    public float mouseSensitivity = 0.1f;
    public float rollSpeed = 60f;

    [Header("Physics")]
    public float gravity = 9.81f;
    public float stabilityThreshold = 0.3f; 

    [Header("Animation")]
    public Animator animator;
    public string animationMultiplierParam = "WingSpeed"; 
    public float normalWingSpeed = 1.0f;
    public float fallingWingSpeed = 0f;
    public float animLerpSpeed = 1f; 

    private CharacterController controller;
    private NewQuaternion currentOrientation;
    private float verticalVelocity = 0f;
    private float currentWingSpeed;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        currentWingSpeed = normalWingSpeed;
    }

    void OnEnable() { Look.Enable(); Move.Enable(); RollInput.Enable(); Cursor.lockState = CursorLockMode.Locked; }

    void OnDisable() { Look.Disable(); Move.Disable(); RollInput.Disable(); Cursor.lockState = CursorLockMode.None; }

    void Start()
    {
        currentOrientation = new NewQuaternion(transform.rotation.w, transform.rotation.x, transform.rotation.y, transform.rotation.z);
    }

    void Update()
    {
        HandleRotation();
        HandleMovement();
        UpdateAnimations();
    }

    void HandleRotation()
    {
        // myszka
        Vector2 lookDelta = Look.ReadValue<Vector2>();
        // q i e
        float rollVal = RollInput.ReadValue<float>();

        // rotacje wokół osi Y (yaw), X (pitch) i Z (roll)
        NewQuaternion yaw = NewQuaternion.FromAngleAxis(lookDelta.x * mouseSensitivity, Vector3.up);
        NewQuaternion pitch = NewQuaternion.FromAngleAxis(-lookDelta.y * mouseSensitivity, Vector3.right);
        NewQuaternion roll = NewQuaternion.FromAngleAxis(-rollVal * rollSpeed * Time.deltaTime, Vector3.forward);


        // składanie rotacji
        currentOrientation = currentOrientation * yaw * pitch * roll;
        currentOrientation = currentOrientation.Normalize();
        
        // aplikacja do transformacji
        transform.rotation = currentOrientation.ToUnityQuaternion();
    }

    void HandleMovement()
    {
        // w a s d space ctrl
        Vector3 input = Move.ReadValue<Vector3>();

        // czy pszczoła ma nośność (czy jest "prawidłowo" ustawiona)
        float liftFactor = Vector3.Dot(transform.up, Vector3.up);

        //reset na ziemi
        if (controller.isGrounded && liftFactor > stabilityThreshold)
        {
            verticalVelocity = 0; 
        }
        // opadanie przy braku nośności
        else if (liftFactor < stabilityThreshold)
        {
            verticalVelocity -= gravity * Time.deltaTime;
        }
         // odzyskiwanie stabilności
        else
        {
            verticalVelocity = Mathf.Lerp(verticalVelocity, 0, Time.deltaTime * 2f);
        }

        // ruch w przestrzeni świata
        Vector3 worldMove = transform.TransformDirection(input * flySpeed);
        worldMove.y += verticalVelocity;

        controller.Move(worldMove * Time.deltaTime);
    }

    void UpdateAnimations()
    {
        if (animator == null) return;

        // sprawdzamy czy pszczoła spada
        float liftFactor = Vector3.Dot(transform.up, Vector3.up);
        
        // wolniejsza animacja przy opadaniu
        float targetSpeed = (liftFactor < stabilityThreshold) ? fallingWingSpeed : normalWingSpeed;

        // płynne przejście
        currentWingSpeed = Mathf.Lerp(currentWingSpeed, targetSpeed, Time.deltaTime * animLerpSpeed);
        animator.SetFloat(animationMultiplierParam, currentWingSpeed);
    }
}