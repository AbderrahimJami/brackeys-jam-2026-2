using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class PlayerAudio : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Rigidbody player_rigidbody;

    [Header("FMOD")]
    [SerializeField] private EventReference footstepEvent;
    [SerializeField] private EventReference breatheEvent;

    [Header("Surface Detection")]
    [SerializeField] private float raycastDistance = 1.2f;
    [SerializeField] private LayerMask groundLayers;

    [Header("Settings")]
    [SerializeField] private float stepInterval = 0.5f;
    [SerializeField] private float minSpeedToStep = 0.1f;
    [SerializeField] private float distance = 0.1f;
    [SerializeField] private float breatheInterval = 1.5f;
    [Range(0f, 1f)]
    [SerializeField] private float breatheVolume = 0.7f;

    private float stepTimer;
    private float breatheTimer;

    private void Update()
    {
        HandleFootsteps();
        HandleBreathing();
    }

    private void HandleFootsteps()
    {
        Vector3 velocity = player_rigidbody.linearVelocity;

        // Avoid magnitude square root.
        float horizontalSpeedSqr =
            velocity.x * velocity.x +
            velocity.z * velocity.z;

        float minSpeedSqr = minSpeedToStep * minSpeedToStep;

        if (horizontalSpeedSqr < minSpeedSqr)
        {
            stepTimer = 0f;
            return;
        }

        stepTimer -= Time.deltaTime;

        if (stepTimer > 0f)
            return;

        PlayFootstep();

        stepTimer = stepInterval;
    }

    private void HandleBreathing()
    {
        breatheTimer -= Time.deltaTime;

        if (breatheTimer > 0f)
            return;

        PlayBreathe();

        breatheTimer = breatheInterval;
    }

    private string DetectSurface()
    {
        Vector3 origin = transform.position + Vector3.up * 0.1f;

        if (Physics.Raycast(
            origin,
            Vector3.down,
            out RaycastHit hit,
            raycastDistance,
            groundLayers))
        {
            if (hit.collider.CompareTag("Wood"))
                return "Wood";

            if (hit.collider.CompareTag("Carpet"))
                return "Carpet";

            if (hit.collider.CompareTag("Stone"))
                return "Stone";
        }

        return "Default";
    }

    private void PlayFootstep()
    {
        EventInstance instance =
            RuntimeManager.CreateInstance(footstepEvent);

        instance.set3DAttributes(
            RuntimeUtils.To3DAttributes(transform)
        );

        instance.setParameterByName(
            "Distance",
            distance
        );

        instance.setParameterByNameWithLabel(
            "Surface",
            DetectSurface()
        );

        instance.start();
        instance.release();
    }

    private void PlayBreathe()
    {
        EventInstance instance =
            RuntimeManager.CreateInstance(breatheEvent);

        instance.set3DAttributes(
            RuntimeUtils.To3DAttributes(transform)
        );

        instance.setParameterByName(
            "BreatheVolume",
            breatheVolume
        );

        instance.start();
        instance.release();
    }
}