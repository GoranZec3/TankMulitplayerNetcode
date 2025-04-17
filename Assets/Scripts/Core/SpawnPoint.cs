using System.Collections.Generic;
using UnityEngine;


public class SpawnPoint : MonoBehaviour
{
    private static List<SpawnPoint> spawnPoints = new List<SpawnPoint>();
 
    [SerializeField] private SphereCollider spawnTrigger;
    [SerializeField] private Team spawnTeam = Team.Deathmatch;

    private bool isOccupied = false;
    private ulong? occupyingClientId = null;

    
    private void Awake()
    {
        if (!spawnPoints.Contains(this))
            spawnPoints.Add(this);

        LookAtCenter();
    }

    private void OnDestroy()
    {
        if (spawnPoints.Contains(this))
            spawnPoints.Remove(this);
    }

     private void LookAtCenter()
    {
        // Set the rotation of the spawn point to face the center of the map
        Vector3 directionToCenter = (Vector3.zero - transform.position).normalized;
        transform.rotation = Quaternion.LookRotation(directionToCenter);
    }

    // public static (Vector3 position, Quaternion rotation) GetRandomSpawnPoint()
    // {
    //     var freeSpawns = spawnPoints.FindAll(spawn => !spawn.isOccupied);

    //     if (freeSpawns.Count == 0)
    //     {
    //         Debug.LogWarning("No free spawn points available!");
    //         return (Vector3.zero, Quaternion.identity);
    //     }

    //     SpawnPoint selected = freeSpawns[Random.Range(0, freeSpawns.Count)];
    //     return (selected.transform.position, selected.transform.rotation);
    // }

    public static (Vector3 position, Quaternion rotation) GetRandomSpawnPoint(Team team)
    {
        var freeSpawns = spawnPoints.FindAll(spawn => 
            !spawn.isOccupied && spawn.spawnTeam == team);

        if (freeSpawns.Count == 0)
        {
            Debug.LogWarning($"No free spawn points available for team {team}!");
            return (Vector3.zero, Quaternion.identity);
        }

        SpawnPoint selected = freeSpawns[Random.Range(0, freeSpawns.Count)];
        return (selected.transform.position, selected.transform.rotation);
    }
    

    private void OnTriggerEnter(Collider other)
    {
        TankPlayer player = other.GetComponentInParent<TankPlayer>();
        if (player != null)
        {
            isOccupied = true;
            occupyingClientId = player.OwnerClientId;
        }
    }

    private void OnTriggerExit(Collider other)
    {
         TankPlayer player = other.GetComponentInParent<TankPlayer>();
        if (player != null && player.OwnerClientId == occupyingClientId)
        {
            isOccupied = false;
            occupyingClientId = null;
        }
    }

    //if client leave game while he's on spawn point
    public static void ClearSpawnForClient(ulong clientId)
    {
        foreach (var spawn in spawnPoints)
        {
            if (spawn.occupyingClientId == clientId)
            {
                spawn.isOccupied = false;
                spawn.occupyingClientId = null; 
                break;
            }
        }
    }

    private void OnDrawGizmos() 
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, 1);
    }
}
