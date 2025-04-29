using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;



public class KillLeaderboardManager : NetworkBehaviour
{
    [SerializeField] private Transform leaderboardEntityHolder;
    [SerializeField] private KillLeaderboardDisplay leaderboardEntityPrefab;
    [SerializeField] private int entitiesToDisplay = 8;

    private NetworkList<KillerEntity> leaderboardEntities;
    private List<KillLeaderboardDisplay> entityDisplays = new List<KillLeaderboardDisplay>();

    private void Awake()
    {
        leaderboardEntities = new NetworkList<KillerEntity>();
    }

    public override void OnNetworkSpawn()
    {
        if (IsClient)
        {
            leaderboardEntities.OnListChanged += HandleLeaderboardEntitiesChanged;
            foreach (KillerEntity entity in leaderboardEntities)
            {
                HandleLeaderboardEntitiesChanged(new NetworkListEvent<KillerEntity>
                {
                    Type = NetworkListEvent<KillerEntity>.EventType.Add,
                    Value = entity
                });
            }
        }

        if (IsServer)
        {
            TankPlayer[] players = FindObjectsByType<TankPlayer>(FindObjectsSortMode.None);
            foreach (TankPlayer player in players)
            {
                HandlePlayerSpawned(player);
            }

            TankPlayer.OnPlayerSpawned += HandlePlayerSpawned;
            TankPlayer.OnPlayerDespawned += HandlePlayerDespawnd;
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsClient)
        {
            leaderboardEntities.OnListChanged -= HandleLeaderboardEntitiesChanged;
        }

        TankPlayer.OnPlayerSpawned -= HandlePlayerSpawned;
        TankPlayer.OnPlayerDespawned -= HandlePlayerDespawnd;
    }

    private void HandleLeaderboardEntitiesChanged(NetworkListEvent<KillerEntity> changeEvent)
    {
        // Safety check -> bug if you fire projectile and leave game explosion get stuck 
        if (!gameObject.scene.isLoaded) { return; }

        switch (changeEvent.Type)
        {
            case NetworkListEvent<KillerEntity>.EventType.Add:
 
                if (!entityDisplays.Any(x => x.ClientId == changeEvent.Value.ClientId))
                {
                    
                    KillLeaderboardDisplay leaderboardEntity = Instantiate(leaderboardEntityPrefab, leaderboardEntityHolder);
              
                       leaderboardEntity.Initialise(
                        changeEvent.Value.ClientId,
                        changeEvent.Value.PlayerName,
                        changeEvent.Value.TotalKills);

                    entityDisplays.Add(leaderboardEntity);                  
                }
                break;

            case NetworkListEvent<KillerEntity>.EventType.Remove:
                KillLeaderboardDisplay displayToRemove = entityDisplays.FirstOrDefault(x => x.ClientId == changeEvent.Value.ClientId);
                if (displayToRemove != null)
                {
                    displayToRemove.transform.SetParent(null);
                    Destroy(displayToRemove.gameObject);
                    entityDisplays.Remove(displayToRemove);
                }
                break;

            case NetworkListEvent<KillerEntity>.EventType.Value:
                KillLeaderboardDisplay displayToUpdate = entityDisplays.FirstOrDefault(x => x.ClientId == changeEvent.Value.ClientId);
                if (displayToUpdate != null)
                {
                    displayToUpdate.UpdateKills(changeEvent.Value.TotalKills);
                }
                break;
        }

        // Sort leaderboard based on TotalKills
        entityDisplays.Sort((first, second) => second.TotalKills.CompareTo(first.TotalKills));

        // Update leaderboard UI
        for (int i = 0; i < entityDisplays.Count; i++)
        {
            
            entityDisplays[i].transform.SetSiblingIndex(i);
            entityDisplays[i].UpdateText();
            // Hide player below 8
            entityDisplays[i].gameObject.SetActive(i <= entitiesToDisplay - 1);
        }

        // Ensure the local player display is correctly shown
        KillLeaderboardDisplay myDisplay = entityDisplays.FirstOrDefault(x => x.ClientId == NetworkManager.Singleton.LocalClientId);

        if (myDisplay != null)
        {
            if (myDisplay.transform.GetSiblingIndex() >= entitiesToDisplay)
            {
   
                leaderboardEntityHolder.GetChild(entitiesToDisplay - 1).gameObject.SetActive(false);
                myDisplay.gameObject.SetActive(true);
            }
        }
    }

    private void HandlePlayerSpawned(TankPlayer player)
    {
        
        leaderboardEntities.Add(new KillerEntity
        {
            ClientId = player.OwnerClientId,
            PlayerName = player.PlayerName.Value,
            TotalKills = 0
        });

  

        player.KillTracker.TotalKills.OnValueChanged += (oldKills, newKills) => HandleKillsChange(player.OwnerClientId, newKills);
        
    }

    private void HandlePlayerDespawnd(TankPlayer player)
    {
        if (leaderboardEntities == null) { return; }

        foreach (KillerEntity entity in leaderboardEntities)
        {
            if (entity.ClientId != player.OwnerClientId) { continue; }

            leaderboardEntities.Remove(entity);
            break;
        }
        player.KillTracker.TotalKills.OnValueChanged -= (oldKills, newKills) => HandleKillsChange(player.OwnerClientId, newKills);
    }

    private void HandleKillsChange(ulong clientId, int newKills)
    {
        for (int i = 0; i < leaderboardEntities.Count; i++)
        {
            if (leaderboardEntities[i].ClientId != clientId) { continue; }
            leaderboardEntities[i] = new KillerEntity
            {
                ClientId = leaderboardEntities[i].ClientId,
                PlayerName = leaderboardEntities[i].PlayerName,
                TotalKills = newKills
            };
            return;
        }
    }
}
