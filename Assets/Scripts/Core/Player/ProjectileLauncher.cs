using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;

public class ProjectileLauncher : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private InputReader inputReader;
    [SerializeField] private CoinWallet wallet;
    [SerializeField] private Transform projectileSpawnPoint;
    [SerializeField] private GameObject serverProjectilePrefab;
    [SerializeField] private GameObject clientProjectilePrefab;
    [SerializeField] private GameObject MuzzleFlash;
    [SerializeField] private Collider playerCollider;

    [Header("Settings")]
    [SerializeField] private float projectileSpeed;
    [SerializeField] private float fireRate;
    [SerializeField] private int costToFire;

    
    private bool isPointerOverUI;
    private bool shouldFire;
    private float timer;
  



    public override void OnNetworkSpawn()
    {
        if(!IsOwner){return;}

        inputReader.PrimaryFireEvent += HandlePrimaryFire;
    }

    public override void OnNetworkDespawn()
    {
        if(!IsOwner){return;}
        inputReader.PrimaryFireEvent -= HandlePrimaryFire;
    }


    private void Update()
    {

        if(!IsOwner){return;}

        isPointerOverUI = EventSystem.current.IsPointerOverGameObject();
        if(timer > 0){timer -= Time.deltaTime;}
        
        if(!shouldFire){return;}

        //projectile fire rate
        if(timer > 0){return;}
        
        if(wallet.TotalCoins.Value < costToFire) { return;}

        PrimaryFireServerRpc(projectileSpawnPoint.position, projectileSpawnPoint.forward);
        SpawnDummyProjectile(projectileSpawnPoint.position, projectileSpawnPoint.forward);


        //fire rate
        timer = 1/fireRate;
     
    }
   
    private void HandlePrimaryFire(bool shouldFire)
    {
        if(shouldFire)
        {
            if(isPointerOverUI){return;}
        }
        this.shouldFire = shouldFire;
    }

    [ServerRpc]
    private void PrimaryFireServerRpc(Vector3 spawnPos, Vector3 direction)
    {
        if(wallet.TotalCoins.Value < costToFire){return;}

        wallet.SpendCoins(costToFire);

        GameObject projectileInstance = Instantiate(serverProjectilePrefab, spawnPos, Quaternion.LookRotation(direction));
        projectileInstance.transform.forward = direction;

        Physics.IgnoreCollision(playerCollider, projectileInstance.GetComponent<Collider>());

        if(projectileInstance.TryGetComponent<DealDamageOnContact>(out DealDamageOnContact dealDamage))
        {
            dealDamage.SetOwner(OwnerClientId);
        }

        if(projectileInstance.TryGetComponent<Rigidbody>(out Rigidbody rb))
        {
            rb.linearVelocity = rb.transform.forward * projectileSpeed;
        }

        SpawnDummyProjectileClientRpc(spawnPos, direction);
    }

    [ClientRpc]
    private void SpawnDummyProjectileClientRpc(Vector3 spawnPos, Vector3 direction)
    {
        if(IsOwner){return;}
        SpawnDummyProjectile(spawnPos, direction);
    }


    private void SpawnDummyProjectile(Vector3 spawnPos, Vector3 direction)
    {
        if(MuzzleFlash != null){Instantiate(MuzzleFlash, spawnPos, Quaternion.LookRotation(direction));}

        GameObject projectileInstance = Instantiate(clientProjectilePrefab, spawnPos, Quaternion.identity);

        projectileInstance.transform.forward = direction;

        Physics.IgnoreCollision(playerCollider, projectileInstance.GetComponent<Collider>());

        if(projectileInstance.TryGetComponent<Rigidbody>(out Rigidbody rb))
        {
            rb.linearVelocity = rb.transform.forward * projectileSpeed;
        }
    }
}
