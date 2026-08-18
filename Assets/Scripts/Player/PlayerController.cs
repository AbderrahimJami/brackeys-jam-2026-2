using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Events;


public class PlayerController : MonoBehaviour
{


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

    bool doJump = false;
    bool sprinting = false;


    

    Vector3 userInput = Vector3.zero;

    Rigidbody rb;
    CapsuleCollider collider;

    CameraController cameraController;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start(){

        
        rb = GetComponent<Rigidbody>();
        collider = GetComponent<CapsuleCollider>();

        cameraController = camera.GetComponent<CameraController>();

        cameraController.initialize(cameraSensitivity, gameObject);

        Cursor.lockState = CursorLockMode.Locked;

        cameraController.setHeadBobbing(true, idle.frequency, idle.amplitude);

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

        if (Input.GetKeyDown(KeyCode.Space) && IsPlayerGrounded())
        {
            doJump = true;
        }

        sprinting = Input.GetKey(KeyCode.LeftShift);


        userInput = (transform.right * moveX) + (transform.forward * moveZ);

        userInput.Normalize();


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
