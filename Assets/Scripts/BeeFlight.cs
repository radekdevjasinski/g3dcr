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

    [Header("Game Rules")]
    public float maxAltitude = 10f; 
    public LayerMask groundLayer;
    public bool isGameOver = false; 

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
        // jeśli gra się skończyła, nadal obsługujemy spadanie, ale nie rotację
        if (!isGameOver)
        {
            HandleRotation();
        }
        
        HandleMovement();
        UpdateAnimations();
    }
    // sprawdza, czy pszczoła powinna zacząć swobodnie spadać
    bool ShouldFreeFall()
    {
        // koniec gry
        if (isGameOver) return true;

        // sprawdzenie rotacji 
        float liftFactor = Vector3.Dot(transform.up, Vector3.up);
        if (liftFactor < stabilityThreshold) return true;

        // sprawdzenie wysokości od terenu
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, Mathf.Infinity, groundLayer))
        {
            if (hit.distance > maxAltitude) return true;
        }

        return false;
    }

    void HandleRotation()
    {
        Vector2 lookDelta = Look.ReadValue<Vector2>();
        float rollVal = RollInput.ReadValue<float>();

        NewQuaternion yaw = NewQuaternion.FromAngleAxis(lookDelta.x * mouseSensitivity, Vector3.up);
        NewQuaternion pitch = NewQuaternion.FromAngleAxis(-lookDelta.y * mouseSensitivity, Vector3.right);
        NewQuaternion roll = NewQuaternion.FromAngleAxis(-rollVal * rollSpeed * Time.deltaTime, Vector3.forward);

        currentOrientation = currentOrientation * yaw * pitch * roll;
        currentOrientation = currentOrientation.Normalize();
        
        transform.rotation = currentOrientation.ToUnityQuaternion();
    }

    void HandleMovement()
    {
        // sprawdzanie swobodnego spadania
        bool isFalling = ShouldFreeFall();

        // jeśli gra się skończyła, ignorujemy input ruchu
        Vector3 input = isGameOver ? Vector3.zero : Move.ReadValue<Vector3>();

        // reset na ziemi
        if (controller.isGrounded && !isFalling)
        {
            verticalVelocity = 0; 
        }
        // opadanie
        else if (isFalling)
        {
            verticalVelocity -= gravity * Time.deltaTime;
        }
        // odzyskiwanie stabilności
        else
        {
            verticalVelocity = Mathf.Lerp(verticalVelocity, 0, Time.deltaTime * 2f);
        }

        Vector3 worldMove = transform.TransformDirection(input * flySpeed);
        worldMove.y += verticalVelocity;

        controller.Move(worldMove * Time.deltaTime);
    }

    // aktualizacja animacji skrzydeł
    void UpdateAnimations()
    {
        if (animator == null) return;

        bool isFalling = ShouldFreeFall();
        
        float targetSpeed = isFalling ? fallingWingSpeed : normalWingSpeed;

        currentWingSpeed = Mathf.Lerp(currentWingSpeed, targetSpeed, Time.deltaTime * animLerpSpeed);
        animator.SetFloat(animationMultiplierParam, currentWingSpeed);
    }
    
    public void EndGame()
    {
        isGameOver = true;
    }
}