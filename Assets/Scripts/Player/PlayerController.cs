using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.Assertions;



public class PlayerController : MonoBehaviour
{

    public static PlayerController Instance { get; private set; }

    [Header("Player Setting")]
    [SerializeField]
    float walkSpeed = 5f;
    [SerializeField]
    float sprintSpeed = 10f;

    [SerializeField]
    float fixedJumpForce = 15f;
    
    [SerializeField]
    float gravityWhenJumping = 1f;
    
    [SerializeField]
    float gravityWhenFalling = 1f;

    [SerializeField]
    float groundCheckDist = 0.7f;

    [SerializeField]
    LayerMask whatCanUserStandOn;


    [Header("Camera Setting")]
    [SerializeField]
    Camera camera;
    [SerializeField]
    float cameraSensitivity = 100f;
    
    [System.Serializable]
    private struct HeadBobbingProfiles
    {
        public float frequency;
        public float amplitude;
    }

    [SerializeField]
    HeadBobbingProfiles idle;
    [SerializeField]
    HeadBobbingProfiles walking;
    [SerializeField]
    HeadBobbingProfiles running;



    [Header("Interaction Settings")]
    [SerializeField]
    float maxInteractionDist = 10f;

    [SerializeField]
    Image crosshair = null;
    [SerializeField]
    LayerMask interactableMask;

    bool doJump = false;
    bool sprinting = false;


    

    Vector3 userInput = Vector3.zero;

    Rigidbody rb;
    CapsuleCollider collider;

    CameraController cameraController = null;
    Inventory inventory;


    public Inventory getInventory()
    {
        return inventory;
    }

    private void Awake()
    {
        Instance = this;
    }

    private void OnValidate()
    {
        if (cameraController != null)
        {
            cameraController.initialize(cameraSensitivity, gameObject);
            cameraController.setHeadBobbing(true, idle.frequency, idle.amplitude);
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start(){

        
        rb = GetComponent<Rigidbody>();
        collider = GetComponent<CapsuleCollider>();
        cameraController = camera.GetComponent<CameraController>();
        inventory = new Inventory();
        
        Assert.IsTrue(rb != null, "RB cannot be NULL");
        Assert.IsTrue(collider != null, "collider cannot be NULL");
        Assert.IsTrue(crosshair != null, "Crosshair image cannot be NULL");

        cameraController.initialize(cameraSensitivity, gameObject);
        cameraController.setHeadBobbing(true, idle.frequency, idle.amplitude);

        Cursor.lockState = CursorLockMode.Locked;

    }

 
    void handleInteractions()
    {
        
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

        if (Physics.Raycast(ray, out RaycastHit hit, maxInteractionDist, interactableMask))
        {
            InteractionInterface i = hit.transform.gameObject.GetComponent<InteractionInterface>();
            crosshair.color = i != null ? Color.red : Color.black;

            if (i != null && Input.GetKeyDown(KeyCode.E))
                i.Interact(gameObject);
        }
        else
        {
            crosshair.color = Color.black;
        }
    }

    bool IsPlayerGrounded()
    {
        Vector3 worldCenter = transform.TransformPoint(collider.center);

        float radius = collider.radius;
        float halfHeight = collider.height * 0.5f;

        Vector3 bottom = worldCenter + Vector3.down * (halfHeight - radius);

        return Physics.SphereCast(
            bottom,
            radius * 0.9f,
            Vector3.down,
            out RaycastHit hit,
            groundCheckDist,
            whatCanUserStandOn
        );

    }


    // Update is called once per frame
    void Update()
    {

        float moveX = Input.GetAxisRaw("Horizontal");
        float moveZ = Input.GetAxisRaw("Vertical");

        userInput = (transform.right * moveX) + (transform.forward * moveZ);
        userInput.Normalize();

        // handling sprint input
        sprinting = Input.GetKey(KeyCode.LeftShift);

        // handling jump input
        if (Input.GetKeyDown(KeyCode.Space) && IsPlayerGrounded())
            doJump = true;
        

        // handling interactions
        handleInteractions();


    }


    private Vector3 handleJumpAndFall(Vector3 velocity)
    {

        if (doJump)
        {
            doJump = false;
            velocity.y = fixedJumpForce;
        }


        // falling and jumping conditions
        if (rb.linearVelocity.y > 0f)
        {
            // going up
            velocity.y += Physics.gravity.y * gravityWhenJumping;
        }

        else if (rb.linearVelocity.y < 0f)
        {
            // falling
            velocity.y += Physics.gravity.y * gravityWhenFalling;
        }


        return velocity;
    }


    private void handleHeadBobbing()
    {
        if (userInput.x == 0f && userInput.z == 0f)
        {

            cameraController.setHeadBobbing(true, idle.frequency, idle.amplitude);
        }
        else
        {
            cameraController.setHeadBobbing(true, walking.frequency, walking.amplitude);
        }
    }

    private void FixedUpdate()
    {

        // movement related
        Vector3 velocity = sprinting ? userInput * sprintSpeed : userInput * walkSpeed;


        velocity.y = rb.linearVelocity.y;

        // jump and fall
        velocity = handleJumpAndFall(velocity);

        // head bobbing
        handleHeadBobbing();


        rb.linearVelocity = velocity;

    }
}
