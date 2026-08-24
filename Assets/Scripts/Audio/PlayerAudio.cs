using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class FootstepAudio : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Rigidbody player_rigidbody;

    [Header("FMOD")]
    [SerializeField] private EventReference footstepEvent;

    [Header("Surface Detection")]
    [SerializeField] private float raycastDistance = 1.2f;
    [SerializeField] private LayerMask groundLayers;

    [Header("Settings")]
    [SerializeField] private float stepInterval = 0.5f;
    [SerializeField] private float minSpeedToStep = 0.1f;
    [SerializeField] private float distance = 0.1f;


    private float stepTimer;

    void Update()
    {
        RuntimeManager.StudioSystem.setParameterByName("Distance", distance);

        Vector3 horizontalVelocity = new Vector3(
            player_rigidbody.linearVelocity.x,
            0f,
            player_rigidbody.linearVelocity.z
        );

        float speed = horizontalVelocity.magnitude;

        if (speed < minSpeedToStep)
        {
            stepTimer = 0f; 
            return;
        }

        stepTimer -= Time.deltaTime;
        if (stepTimer <= 0f)
        {
            PlayFootstep();
            stepTimer = stepInterval;
        }
    }

    string DetectSurface()
    {
        RaycastHit hit;
        Vector3 origin = transform.position + Vector3.up * 0.1f;    

        if (Physics.Raycast(origin, Vector3.down, out hit, raycastDistance, groundLayers))
        {
            switch (hit.collider.tag)
            {                
                case "Wood": return "Wood";
                case "Carpet": return "Carpet";
                case "Stone": return "Stone";
                default: return "Default";
            }
        }    
        return "Default";
    }

    void PlayFootstep()
    {  

        EventInstance instance = RuntimeManager.CreateInstance(footstepEvent);
        RuntimeManager.StudioSystem.setParameterByNameWithLabel("Surface", DetectSurface());
        instance.set3DAttributes(RuntimeUtils.To3DAttributes(transform));
        instance.start();
        instance.release();        
    }
}