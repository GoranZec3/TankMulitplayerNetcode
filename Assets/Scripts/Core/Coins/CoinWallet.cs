using System;
using Unity.Netcode;
using UnityEngine;

public class CoinWallet : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private Health health;
    [SerializeField] private BountyCoin coinPrefab;

    [Header("Settings")]
    [SerializeField] private float coinSpread = 3f;
    [SerializeField] private float bountyPercentage = 50f;
    [SerializeField] private int bountyCoinCount = 10;
    [SerializeField] private int minBountyCoinValue = 5;
    [SerializeField] private LayerMask layerMask;
    private float coinRadius;
    private Collider[] coinBuffer = new Collider[1];
    
    


    public NetworkVariable<int> TotalCoins = new NetworkVariable<int>();



    public override void OnNetworkSpawn()
    {
        
        if(!IsServer){return;}

        coinRadius = coinPrefab.GetComponent<SphereCollider>().radius;

        health.OnDie += HandleDie;
    }

    public override void OnNetworkDespawn()
    {
        if(!IsServer){return;}
        health.OnDie -= HandleDie;
    }


    public void SpendCoins(int costToFire)
    {
        TotalCoins.Value -= costToFire;
    }

    private void OnTriggerEnter(Collider collider)
    {
        if(!collider.TryGetComponent<Coin>(out Coin coin)){return;}

        int coinValue = coin.Collect();

        if(!IsServer){return;}

        TotalCoins.Value += coinValue;
    }

    private void HandleDie(Health health)
    {
        int bountyValue = (int) (TotalCoins.Value * (bountyPercentage / 100f));
        int bountyCoinValue = bountyValue / bountyCoinCount;

        if(bountyCoinValue < minBountyCoinValue){return;}

        for (int i = 0; i < bountyCoinCount; i++)
        {
            BountyCoin coinInstance = Instantiate(coinPrefab, GetSpawnPoint(), Quaternion.identity);
            coinInstance.SetValue(bountyCoinValue);
            coinInstance.NetworkObject.Spawn();
        }
    }

    private Vector3 GetSpawnPoint()
    {

        //had problem with terrain collision had to put terran in its own layer
        int maxAttempts = 1000; // Prevents infinite loops
        for (int i = 0; i < maxAttempts; i++)
        {
            Debug.Log("Start searching position ");
            Vector2 randomCircle = UnityEngine.Random.insideUnitCircle * coinSpread;
            Vector3 spawnPoint = transform.position + new Vector3(randomCircle.x, 0f, randomCircle.y);


            int numCollider = Physics.OverlapSphereNonAlloc(spawnPoint, coinRadius, coinBuffer, layerMask);
            if (numCollider == 0)
            {
                return spawnPoint;
            }
        }

        Debug.LogWarning("Failed to find a valid spawn point after max attempts. Returning default position.");
        return transform.position; // Return default position if no valid spot found

    }
}
