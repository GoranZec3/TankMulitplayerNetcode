using System;
using Unity.Cinemachine;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class TankPlayer : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private CinemachineCamera cinemachineCamera;
    [SerializeField] private CinemachineOrbitalFollow orbitalFollow;
    [SerializeField] private SpriteRenderer minimapIconRenderer;
    [SerializeField] private Texture2D crosshair;
    [SerializeField] private Transform bodyTransfrom;
    [field:SerializeField] public Health Health{get; private set;} //property which is shown in inspctor - using 'field:'
    [field:SerializeField] public CoinWallet Wallet{get; private set;} 
    

    [Header("Settings")]
    [SerializeField] private int ownerPriority = 15;
    [SerializeField] private Color ownerColor;

    public NetworkVariable<FixedString32Bytes> PlayerName = new NetworkVariable<FixedString32Bytes>();

    public static event Action<TankPlayer> OnPlayerSpawned;
    public static event Action<TankPlayer> OnPlayerDespawned;


     public NetworkVariable<float> cameraRotationY = new NetworkVariable<float>();

  
    private void SetCameraRotation(float rotationY)
    {
        // Update the NetworkVariable when the rotation changes
        if (IsServer)
        {
            cameraRotationY.Value = rotationY;
        }
    }

    public override void OnNetworkSpawn()
    {
        if(IsServer)
        {
            UserData userData = null;
            if(IsHost)
            {
                userData = HostSingleton.Instance.GameManager.NetworkServer.GetUserDataByClientId(OwnerClientId);
            }
            
            PlayerName.Value = userData.userName;

            Debug.Log($"Player {userData.userName} (Client {OwnerClientId}) is in Team {userData.teamId}");
            OnPlayerSpawned?.Invoke(this);
        }

        if(IsOwner)
        {
            cameraRotationY.OnValueChanged += HandleCameraRotationChanged;
            cinemachineCamera.Priority = ownerPriority;  
            minimapIconRenderer.color = ownerColor;  
            Cursor.SetCursor(crosshair, new Vector2(crosshair.width/2, crosshair.height/2), CursorMode.Auto); 

            
        }
    }


    private void HandleCameraRotationChanged(float oldRotation, float newRotation)
    {
        // Update the CinemachineOrbitalFollow horizontal axis on the client
        var orbitalFollow = GetComponentInChildren<CinemachineOrbitalFollow>();
        if (orbitalFollow != null)
        {
            orbitalFollow.HorizontalAxis.Value = newRotation;
        }
    }

    // Set the rotation 
    public void SetCameraRotationOnAllClients(float rotationY)
    {
        SetCameraRotation(rotationY);  
    }

    public override void OnNetworkDespawn()
    {
        if(IsServer)
        {
            OnPlayerDespawned?.Invoke(this);
            SpawnPoint.ClearSpawnForClient(OwnerClientId);
        }
    }

}
