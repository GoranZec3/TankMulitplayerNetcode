using Unity.Netcode;
using UnityEngine;

public class MinimapCameraSetup : NetworkBehaviour
{
    [SerializeField] private Camera minimapCamera;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
        {
            minimapCamera.gameObject.SetActive(false); 
        }
    }
}
