using System;
using System.Collections;
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
        //way to send parameter to HandlePlayerDie
        player.Health.OnDie += (health) => HandlePlayerDie(player);
    }


    private void HandlePlayerDespawn(TankPlayer player)
    {
        player.Health.OnDie -= (health) => HandlePlayerDie(player);    
    }

    private void HandlePlayerDie(TankPlayer player)
    {   
        int keptCoins = (int)(player.Wallet.TotalCoins.Value * (keptCoinPercentage / 100));

        Destroy(player.gameObject);
        //wait for frame to respawn
        StartCoroutine(RespawnPlayer(player.OwnerClientId, keptCoins));
    }

    private IEnumerator RespawnPlayer(ulong ownerClientId, int keptCoins)
    {
        // yield return null;
        yield return new WaitForSeconds(0.1f);

        TankPlayer playerInstance = Instantiate(playerPrefab, SpawnPoint.GetRandomSpawnPos(), Quaternion.identity);
        
        playerInstance.NetworkObject.SpawnAsPlayerObject(ownerClientId);

        playerInstance.Wallet.TotalCoins.Value += keptCoins;
    }
}
