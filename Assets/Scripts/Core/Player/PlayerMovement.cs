using Unity.Netcode;
using UnityEngine;

public class PlayerMovement : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private InputReader inputReader;
    [SerializeField] private Transform bodyTransfrom;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private ParticleSystem dustCloud;
    [SerializeField] private Animator animator;

    [Header("Settings")]
    [SerializeField] private float acceleration = 15f;
    [SerializeField] private float friction = 3f;
    [SerializeField] private float turningRate = 30f;
    [SerializeField] private float particleEmissionValue = 130f;

    private ParticleSystem.EmissionModule emissionModule;
    private Vector3 previousMovementInput;
    private Vector3 previousPos;

    private Vector3 velocityVector;
    private Vector3 accVector;

    private const float ParticleStopThreshold = 0.001f;

    private void Awake()
    {
        emissionModule = dustCloud.emission;
    }

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) { return; }

        inputReader.MoveEvent += HandleMove;
    }

    public override void OnNetworkDespawn()
    {
        if (!IsOwner) { return; }
        inputReader.MoveEvent -= HandleMove;
    }

    private void Update()
    {
        if (!IsOwner) { return; }
        // Rotate body to set moving direction
        float yRotation = previousMovementInput.x * turningRate * Time.deltaTime;
        bodyTransfrom.Rotate(0f, yRotation, 0f);
    }

    private void FixedUpdate()
    {
        if (!IsOwner) { return; }

        // Calculate desired direction based on input
        Vector3 desiredDirection = bodyTransfrom.forward * previousMovementInput.z;

        // Apply acceleration
        accVector = desiredDirection.normalized * acceleration;
        
         // If moving in reverse (negative Z-axis input), reduce the speed
        float reverseSpeedMultiplier = previousMovementInput.z < 0 ? 0.65f : 1f; 
        velocityVector += accVector * reverseSpeedMultiplier * Time.fixedDeltaTime;

        // Apply friction
        float frictionFactor = 1 - (friction * Time.fixedDeltaTime);
        velocityVector *= Mathf.Clamp01(frictionFactor);

        // Apply movement to Rigidbody
        rb.linearVelocity = velocityVector;

        //Animator
        if(animator != null)
        {
            float currentSpeed = rb.linearVelocity.magnitude;
            animator.speed = currentSpeed > 0.1f ? currentSpeed / 5f : 0f;
        }
        


        // Particle logic
        if ((transform.position - previousPos).sqrMagnitude > ParticleStopThreshold)
        {
            emissionModule.rateOverTime = particleEmissionValue;
        }
        else
        {
            emissionModule.rateOverTime = 0;
        }

        previousPos = transform.position;
    }

    private void HandleMove(Vector3 movementInput)
    {
        previousMovementInput = movementInput;
    }
}
