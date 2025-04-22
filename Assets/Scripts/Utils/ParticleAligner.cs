using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class ParticleAligner : NetworkBehaviour
{
    private ParticleSystem.MainModule psMain;
    private Rigidbody rb;
    private NetworkVariable<float> networkRotation = new NetworkVariable<float>();

    void Start()
    {
        rb = GetComponentInParent<Rigidbody>();
        var ps = GetComponent<ParticleSystem>();
        psMain = ps.main;
        psMain.simulationSpace = ParticleSystemSimulationSpace.World;

        networkRotation.OnValueChanged += OnRotationChanged; //event network
    }

    // void Update()
    // {
    //     Vector3 velocity = rb.linearVelocity;
    //     if (velocity.sqrMagnitude > 0.01f)
    //     {
    //         float angle = Mathf.Atan2(-velocity.x, -velocity.z); // flip for backward trails
    //         psMain.startRotation = angle;
    //     }
    // }   

    void Update()
    {
        if (IsOwner)
        {
            // Only owners calculate and send rotation
            Vector3 velocity = rb.linearVelocity;
            if (velocity.sqrMagnitude > 0.01f)
            {
                float angle = Mathf.Atan2(-velocity.x, -velocity.z);
                if (IsServer)
                {
                    networkRotation.Value = angle;
                }
                else
                {
                    UpdateRotationServerRpc(angle);
                }
                psMain.startRotation = angle;
            }
        }
    }

    [ServerRpc]
    private void UpdateRotationServerRpc(float angle)
    {
        networkRotation.Value = angle;
    }

    private void OnRotationChanged(float oldValue, float newValue)
    {
        if (!IsOwner)
        {
            // Non-owners apply received rotation
            psMain.startRotation = newValue;
        }
    }

    public override void OnDestroy()
    {
        networkRotation.OnValueChanged -= OnRotationChanged;
        base.OnDestroy();
    }


}
