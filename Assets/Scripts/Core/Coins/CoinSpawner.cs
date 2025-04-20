using System;
using Unity.Netcode;
using UnityEngine;

public class CoinSpawner : NetworkBehaviour
{
    [SerializeField] private RespawningCoin coinPrefab;
    [SerializeField] private int maxCoins = 50;
    [SerializeField] private int coinValue  = 10;

    [SerializeField] private Vector3 xSpawnRange;
    [SerializeField] private Vector3 zSpawnRange;

    [SerializeField] private LayerMask layerMask;

    private float coinRadius;
    private Collider[] coinBuffer = new Collider[1];

    public override void OnNetworkSpawn()
    {
        if(!IsServer){return;}

        coinRadius = coinPrefab.GetComponent<SphereCollider>().radius;

        for(int i=0; i<maxCoins; i++)
        {
            SpawnCoin();
        }
    }

    private void  SpawnCoin()
    {
        RespawningCoin coinInstance = Instantiate(coinPrefab, GetSpawnPoint(), Quaternion.identity);

        coinInstance.SetValue(coinValue);
        coinInstance.GetComponent<NetworkObject>().Spawn();
        //if this unsubscribe -> never trigger HandleCoinCollected to reset it -> it would stay in place
        coinInstance.OnCollected += HandleCoinCollected;
    }

    private void HandleCoinCollected(RespawningCoin coin)
    {
        coin.transform.position = GetSpawnPoint();
        coin.Reset();
    }

    private Vector3 GetSpawnPoint()
    {
        for (int i = 0; i < 500; i++) 
        {
            float x = UnityEngine.Random.Range(xSpawnRange.x, xSpawnRange.z);
            float z = UnityEngine.Random.Range(zSpawnRange.x, zSpawnRange.z);

            Vector3 spawnPoint = new Vector3(x, 0, z);
            int numColliders = Physics.OverlapSphereNonAlloc(spawnPoint, coinRadius, coinBuffer, layerMask);

            if (numColliders == 0)
            {
                return spawnPoint;
            }
        }

        // Fallback if we couldn't find a valid position
        Debug.LogWarning("CoinSpawner: Failed to find a valid spawn point after 500 attempts. Returning Vector3.zero.");
        return Vector3.zero;
    }
}
