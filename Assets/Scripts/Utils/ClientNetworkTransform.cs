using Unity.Netcode.Components;
using UnityEngine;

public class ClientNetworkTransform : NetworkTransform
{
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        CanCommitToTransform = IsOwner;
    }

    private void Update()
    {
        if (!IsOwner || NetworkManager == null || !NetworkManager.IsClient)
            return;

        CanCommitToTransform = IsOwner;

        if (NetworkManager.IsConnectedClient || NetworkManager.IsListening)
        {
            if (CanCommitToTransform)
            {
                // SetState() instead of TryCommitTransformToServer()
                SetState(transform.position, transform.rotation, transform.localScale, true);
                
            }
        }
    }

    protected override bool OnIsServerAuthoritative()
    {
        return false;
    }
}
