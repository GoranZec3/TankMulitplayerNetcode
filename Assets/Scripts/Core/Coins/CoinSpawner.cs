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

        coinInstance.OnCollected += HandleCoinCollected;
    }

    private void HandleCoinCollected(RespawningCoin coin)
    {
        coin.transform.position = GetSpawnPoint();
        coin.Reset();
    }

    private Vector3 GetSpawnPoint()
    {
        float x = 0;
        float z = 0;

        while(true)
        {
            x = UnityEngine.Random.Range(xSpawnRange.x, xSpawnRange.z);
            z = UnityEngine.Random.Range(zSpawnRange.x, zSpawnRange.z);

            Vector3 spawnPoint = new Vector3(x, 0, z);
            int numCollider = Physics.OverlapSphereNonAlloc(spawnPoint, coinRadius, coinBuffer, layerMask);
            if(numCollider == 0)
            {
                return spawnPoint;
            }
        }
    }
}
