using System;
using System.Collections;
using Unity.Cinemachine;
using Unity.Netcode;
using UnityEngine;

public class RespawnHandler : NetworkBehaviour
{
    [SerializeField] private TankPlayer playerPrefab;
    [SerializeField] private float keptCoinPercentage;


    public override void OnNetworkSpawn()
    {
        if(!IsServer){return;}
     
        //if you are already in scene
        TankPlayer[] players = FindObjectsByType<TankPlayer>(FindObjectsSortMode.None);
        foreach(TankPlayer player in players)
        {
            HandlePlayerSpawn(player);
        }
        //if you are joined
        TankPlayer.OnPlayerSpawned += HandlePlayerSpawn;
        TankPlayer.OnPlayerDespawned += HandlePlayerDespawn;
    }


    public override void OnNetworkDespawn()
    {
        if(!IsServer){return;}

        TankPlayer.OnPlayerSpawned -= HandlePlayerSpawn;
        TankPlayer.OnPlayerDespawned -= HandlePlayerDespawn;
        
    }

    private void HandlePlayerSpawn(TankPlayer player)
    {
        player.Health.OnDie += OnPlayerDied;
    }

    private void HandlePlayerDespawn(TankPlayer player)
    {
        player.Health.OnDie -= OnPlayerDied;

    }

    private void OnPlayerDied(Health health)
    {
        var player = health.GetComponentInParent<TankPlayer>();
        HandlePlayerDie(player);
    }

    private void HandlePlayerDie(TankPlayer player)
    {   
        int keptCoins = (int)(player.Wallet.TotalCoins.Value * (keptCoinPercentage / 100));
        int keptKills = (int)(player.KillTracker.TotalKills.Value);

        Destroy(player.gameObject);
        //wait for frame to respawn
        StartCoroutine(RespawnPlayer(player.OwnerClientId, keptCoins, keptKills));
    }


    private IEnumerator RespawnPlayer(ulong ownerClientId, int keptCoins, int keptKills)
    {
        yield return new WaitForSeconds(0.1f);

        UserData userData = HostSingleton.Instance.GameManager.NetworkServer.GetUserDataByClientId(ownerClientId);
        Team playerTeam = (Team)userData.teamId;  
         GameObject selectedPrefab = HostSingleton.Instance.GameManager.GetSelectedPlayerPrefab(userData.tankIndex);

        var spawnData = SpawnPoint.GetRandomSpawnPoint(playerTeam); 

        TankPlayer playerInstance = Instantiate(selectedPrefab, spawnData.position, spawnData.rotation).GetComponent<TankPlayer>();

        // Keep old ID to new instance
        playerInstance.NetworkObject.SpawnAsPlayerObject(ownerClientId);

        // Add the coins back to the player
        playerInstance.Wallet.TotalCoins.Value += keptCoins;
        playerInstance.KillTracker.TotalKills.Value += keptKills;

        // Set spawn rotation and camera rotation for all clients after death
        SetSpawnRotation(playerInstance);
    }

    private void SetSpawnRotation(TankPlayer playerInstance)
    {
        float rotationY = playerInstance.transform.eulerAngles.y;
        if (rotationY < 0)
        {
            rotationY += 360;
        }
    
        playerInstance.SetCameraRotationOnAllClients(rotationY);
    }


}
