using Unity.Netcode;
using UnityEngine;

public class PlayerMovement : NetworkBehaviour
{

    [Header("References")]
    [SerializeField] private InputReader inputReader;
    [SerializeField] private Transform bodyTransfrom;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private ParticleSystem dustCloud;


    [Header("Settings")]
    [SerializeField] private float movementSpeed  = 4f;
    [SerializeField] private float turningRate = 30f;
    [SerializeField] private float particleEmissionValue = 130f;

    private ParticleSystem.EmissionModule emissionModule;
    private Vector3 previousMovementInput;
    private Vector3 previousPos;

    private const float ParticleStopThreshold = 0.005f;

    private void Awake()
    {
        emissionModule = dustCloud.emission;
    }

    public override void OnNetworkSpawn()
    {
        if(!IsOwner) {return;}

        inputReader.MoveEvent += HandleMove;
    }

    public override void OnNetworkDespawn()
    {
        if(!IsOwner) {return;}
        inputReader.MoveEvent -= HandleMove;
    }
 
    private void Update()
    {
        if(!IsOwner) {return;}
        //rotate body to set moving direction
        float yRotation = previousMovementInput.x * turningRate * Time.deltaTime;
        bodyTransfrom.Rotate(0f, yRotation ,0f);
    }

    private void FixedUpdate()
    {
        //if we have move since last frame
        if((transform.position - previousPos).sqrMagnitude>ParticleStopThreshold)
        {
            emissionModule.rateOverTime = particleEmissionValue;
        }
        else
        {
            emissionModule.rateOverTime = 0;
        }


        previousPos = transform.position;
        if(!IsOwner){return;}
        Vector3 movementDirection = bodyTransfrom.forward * previousMovementInput.z;
        rb.linearVelocity = movementDirection * movementSpeed;

        //TODO set speed for going reverse to be slow
    }

    private void HandleMove(Vector3 movementInput)
    {
        previousMovementInput = movementInput;
    }


}
