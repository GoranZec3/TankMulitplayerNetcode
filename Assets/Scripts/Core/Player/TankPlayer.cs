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
    [field:SerializeField] public Health Health{get; private set;} //property which is shown in inspctor - using 'field:'
    [field:SerializeField] public CoinWallet Wallet{get; private set;} 
    

    [Header("Settings")]
    [SerializeField] private int ownerPriority = 15;
    [SerializeField] private Color ownerColor;

    public NetworkVariable<FixedString32Bytes> PlayerName = new NetworkVariable<FixedString32Bytes>();

    public static event Action<TankPlayer> OnPlayerSpawned;
    public static event Action<TankPlayer> OnPlayerDespawned;

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

            OnPlayerSpawned?.Invoke(this);
        }

        if(IsOwner)
        {
            cinemachineCamera.Priority = ownerPriority;
            orbitalFollow.HorizontalAxis.Value = 0;   
            minimapIconRenderer.color = ownerColor;  
            Cursor.SetCursor(crosshair, new Vector2(crosshair.width/2, crosshair.height/2), CursorMode.Auto);    
        }
    }

    public override void OnNetworkDespawn()
    {
        if(IsServer)
        {
            OnPlayerDespawned?.Invoke(this);
        }
    }
}
