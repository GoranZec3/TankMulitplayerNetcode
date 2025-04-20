using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class ParticleAligner : MonoBehaviour
{
    private ParticleSystem.MainModule psMain;

    // void Start()
    // {
    //     psMain = GetComponent<ParticleSystem>().main;
    // }

    // void Update()
    // {
    //     // psMain.startRotation = -transform.rotation.eulerAngles.y * Mathf.Deg2Rad;
    //     float angle = Mathf.Atan2(transform.forward.x, transform.forward.z);
    //     psMain.startRotation = angle;
    // }

//with acceleration
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponentInParent<Rigidbody>();
        var ps = GetComponent<ParticleSystem>();
        psMain = ps.main;
        psMain.simulationSpace = ParticleSystemSimulationSpace.World;
    }

    void Update()
    {
        Vector3 velocity = rb.linearVelocity;
        if (velocity.sqrMagnitude > 0.01f)
        {
            float angle = Mathf.Atan2(-velocity.x, -velocity.z); // flip for backward trails
            psMain.startRotation = angle;
        }
    }

    
}
