using System.Collections;
using System.Collections.Generic;
using TMPro;
using TrustNoOne.Shuffle;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;



public class PlayerController : MonoBehaviour
{

    public static PlayerController Instance { get; private set; }

    [Header("Player Setting")]
    [SerializeField]
    float walkSpeed = 5f;
    //[SerializeField]
    //float sprintSpeed = 10f;

    [SerializeField]
    float acceleration = 15f;

    [SerializeField]
    float deceleration = 20f;


    [Header("Camera Setting")]
    [SerializeField]
    Camera camera;
    [SerializeField]
    public float cameraSensitivity = 100f;

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



    [Header("Interaction Settings")]
    [SerializeField]
    float maxInteractionDist = 10f;

    [SerializeField]
    Image crosshair = null;
    [SerializeField]
    LayerMask interactableMask;


    [Header("Sequencial Messages")]
    [SerializeField]
    List<string> storedSequencialMessages = new List<string>();

    int currentSequencialMessageIndex = 0;


    HeadBobbingProfiles activeProfile;
    Vector3 userInput = Vector3.zero;

    Rigidbody rb;
    CapsuleCollider collider;
    Inventory inventory;
    Transform oriantationTransform;
    Camera mainCamera;
    Vector3 mainCameraOriginalPosition;

    bool startHeadBobbing = false;


    float xRotation;
    float yRotation;
    float headBobbingTimer = 0f;


    GameObject teleportLocationObject;


    bool isInitialized = false;


    public bool getIsInitialized()
    {
        return isInitialized;
    }

    public Inventory getInventory()
    {
        return inventory;
    }



    public void TeleportToSafeRoom()
    {
        if (rb == null) rb = GetComponent<Rigidbody>();

        // 1. Find the safe room (Consider caching this list elsewhere if called often)
        RoomInstance[] rooms = FindObjectsByType<RoomInstance>();

        for (int i = 0; i < rooms.Length; i++)
        {
            if (rooms[i].definition.roomType == RoomType.SafeRoom)
            {
                // 2. Calculate the destination world position
                Vector3 targetWorldPos = rooms[i].gameObject.transform.position;
                targetWorldPos.y += 2f;

                // 3. Properly reset physics forces
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero; // Stops any spinning

                // 4. Teleport the Rigidbody directly in world space
                rb.position = targetWorldPos;
                transform.position = targetWorldPos; // Syncs transform immediately

                Debug.Log("Player Teleported to Safe Room");
                return;
            }
        }

        Debug.Log("Safe Room NOT Found!");
    }

    private void Awake()
    {
        Instance = this;

        teleportLocationObject = GameObject.Find("TeleportLocationPlayer");

    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {


        rb = GetComponent<Rigidbody>();
        collider = GetComponent<CapsuleCollider>();
        inventory = new Inventory();
        oriantationTransform = transform.Find("Orientation");

        Assert.IsTrue(rb != null, "RB cannot be NULL");
        Assert.IsTrue(collider != null, "collider cannot be NULL");
        Assert.IsTrue(crosshair != null, "Crosshair image cannot be NULL");


        mainCamera = Camera.main;

        mainCameraOriginalPosition = mainCamera.transform.localPosition;
        

        Cursor.lockState = CursorLockMode.Locked;

        setSensi();

        isInitialized = true;
    }


    void setSensi()
    {
        if (!PlayerPrefs.HasKey("cameraSensitivity"))
        {
            PlayerPrefs.SetFloat("cameraSensitivity", cameraSensitivity);
            PlayerPrefs.Save();
        }
        else
        {
            cameraSensitivity = PlayerPrefs.GetFloat("cameraSensitivity");
        }
    }

    void handleInteractions()
    {

        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

        if (Physics.Raycast(ray, out RaycastHit hit, maxInteractionDist, interactableMask))
        {
            InteractionInterface i = hit.transform.gameObject.GetComponentInParent<InteractionInterface>();
            crosshair.color = i != null ? Color.red : Color.black;

            if (i != null && Input.GetKeyDown(KeyCode.E))
                i.Interact(gameObject);
        }
        else
        {
            crosshair.color = Color.black;
        }
    }



    void lookAround()
    {
        float mouseX = Input.GetAxis("Mouse X") * cameraSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * cameraSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 60f);

        yRotation += mouseX;

        if (startHeadBobbing)
        {
            headBobbingTimer += Time.deltaTime;
            
            float val = activeProfile.amplitude * Mathf.Sin(headBobbingTimer * activeProfile.frequency);

            Vector3 tempPosition = mainCamera.transform.localPosition;
            tempPosition.y = mainCameraOriginalPosition.y + val; 

            mainCamera.transform.localPosition = tempPosition;
        }


        mainCamera.transform.rotation = Quaternion.Euler(xRotation, yRotation, 0f);
        oriantationTransform.rotation = Quaternion.Euler(0f, yRotation, 0f);


    }

    void Update()
    {

        float moveX = Input.GetAxisRaw("Horizontal");
        float moveZ = Input.GetAxisRaw("Vertical");

        Vector3 input = new Vector3(moveX, 0f, moveZ);
        userInput = (oriantationTransform.forward * input.z) + (oriantationTransform.right * input.x);
        
        if (userInput.sqrMagnitude > 1f)
            userInput.Normalize();

        userInput.y = 0f;

        // handle camera + player rotation
        lookAround();

        // handling interactions
        handleInteractions();


        // head bobbing
        handleHeadBobbing();


        // teleportation to saferoom
        if (Input.GetKeyDown(KeyCode.P))
            TeleportToSafeRoom();

    }



    private void handleHeadBobbing()
    {
        if (userInput.sqrMagnitude < 0.01f)
        {
            activeProfile = idle;
            startHeadBobbing = true;
        }
        else
        {
            activeProfile = walking;
            startHeadBobbing = true;
        }
    }

    private void FixedUpdate()
    {

        // movement related
        Vector3 targetVelocity = userInput * walkSpeed;
        Vector3 currentVelocity = rb.linearVelocity;

        Vector3 currentHorizontalVelocity = new Vector3(
            currentVelocity.x,
            0f,
            currentVelocity.z
        );

        float movementRate = userInput.sqrMagnitude > 0.01f ? acceleration : deceleration;

        Vector3 horizontalVelocity = Vector3.MoveTowards(
            currentHorizontalVelocity,
            targetVelocity,
            movementRate * Time.fixedDeltaTime
        );

        Vector3 velocity = horizontalVelocity;

        velocity.y = rb.linearVelocity.y;
        rb.linearVelocity = velocity;


    }

    private IEnumerator fadeInText(TextMeshPro text)
    {
        float fadeInDuration = 2f;
        float timer = 0;

        text.alpha = 0f;
        while (timer < fadeInDuration)
        {
            timer += Time.deltaTime;

            text.alpha += Mathf.Lerp(0f, 1f, timer);

            yield return new WaitForSeconds(0.1f);
        }

    }

    private void OnTriggerEnter(Collider other)
    {

        if (currentSequencialMessageIndex >= storedSequencialMessages.Count) return;

        if (other.gameObject.TryGetComponent<RoomInstance>(out RoomInstance roomInstance))
        {
            SequencialMessage[] messages = roomInstance.gameObject.GetComponentsInChildren<SequencialMessage>();

            for (int i = 0; i < messages.Length; i++)
            {

                TextMeshPro textComponent = messages[i].gameObject.GetComponent<TextMeshPro>();

                if (textComponent != null && textComponent.text.Trim() == "")
                {
                    textComponent.SetText(storedSequencialMessages[currentSequencialMessageIndex++]);
                    StartCoroutine(fadeInText(textComponent));
                    if (currentSequencialMessageIndex >= storedSequencialMessages.Count) return;
                }

            }

        }

    }
}