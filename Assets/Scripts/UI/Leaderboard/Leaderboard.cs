using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class Leaderboard : NetworkBehaviour
{
    [SerializeField] private Transform leaderboardEntityHolder;
    [SerializeField] private LeaderboardEntityDisplay leaderboardEntityPrefab;
    [SerializeField] private int entitiesToDisplay = 8;

    private NetworkList<LeaderboardEntityState> leaderboardEntities;
    private List<LeaderboardEntityDisplay> entityDisplays = new List<LeaderboardEntityDisplay>();

    private void Awake()
    {
        leaderboardEntities = new NetworkList<LeaderboardEntityState>();
    }

    public override void OnNetworkSpawn()
    {

        if(IsClient){
            leaderboardEntities.OnListChanged += HandleLeaderboardEntitiesChanged;
            foreach(LeaderboardEntityState entity in leaderboardEntities)
            {
                HandleLeaderboardEntitiesChanged(new NetworkListEvent<LeaderboardEntityState>
                {
                    Type = NetworkListEvent<LeaderboardEntityState>.EventType.Add,
                    Value = entity
                });
            }
        }

        if(IsServer)
        {
            TankPlayer[] players = FindObjectsByType<TankPlayer>(FindObjectsSortMode.None);
            foreach(TankPlayer player in players)
            {
                HandlePlayerSpawned(player);
            }

            TankPlayer.OnPlayerSpawned += HandlePlayerSpawned;
            TankPlayer.OnPlayerDespawned += HandlePlayerDespawnd;
        }
    }

    public override void OnNetworkDespawn()
    {

        if(IsClient){
            leaderboardEntities.OnListChanged -= HandleLeaderboardEntitiesChanged;
        }

        TankPlayer.OnPlayerSpawned -= HandlePlayerSpawned;
        TankPlayer.OnPlayerDespawned -= HandlePlayerDespawnd;
    }

    private void HandleLeaderboardEntitiesChanged(NetworkListEvent<LeaderboardEntityState> changeEvent)
    {
        //Safety check -> bug if you fire projectile and leave game explosion get stuck 
        if(!gameObject.scene.isLoaded){return;}
        switch(changeEvent.Type)
        {
            case NetworkListEvent<LeaderboardEntityState>.EventType.Add:
                if(!entityDisplays.Any(x => x.ClientId == changeEvent.Value.ClientId))
                {
                    LeaderboardEntityDisplay leaderboardEntity = Instantiate(leaderboardEntityPrefab, leaderboardEntityHolder);

                    leaderboardEntity.Initialise(
                        changeEvent.Value.ClientId, 
                        changeEvent.Value.PlayerName,
                        changeEvent.Value.Coins);
                    entityDisplays.Add(leaderboardEntity);
                }
                break;
            case NetworkListEvent<LeaderboardEntityState>.EventType.Remove:
                LeaderboardEntityDisplay displayToRemove = entityDisplays.FirstOrDefault(x => x.ClientId == changeEvent.Value.ClientId);
                if(displayToRemove != null){
                    displayToRemove.transform.SetParent(null);
                    Destroy(displayToRemove.gameObject);
                    entityDisplays.Remove(displayToRemove);
                }
                break;
            case NetworkListEvent<LeaderboardEntityState>.EventType.Value:
                LeaderboardEntityDisplay displayToUpdate = entityDisplays.FirstOrDefault(x => x.ClientId == changeEvent.Value.ClientId);
                if(displayToUpdate != null)
                {
                    displayToUpdate.UpdateCoins(changeEvent.Value.Coins);
                }
                break;
        }

        entityDisplays.Sort((first, second)=>second.Coins.CompareTo(first.Coins));
        for(int i=0; i<entityDisplays.Count; i++)
        {
            entityDisplays[i].transform.SetSiblingIndex(i);
            entityDisplays[i].UpdateText();
            //hide player below 8
            entityDisplays[i].gameObject.SetActive(i <= entitiesToDisplay - 1);
        }

        LeaderboardEntityDisplay myDisplay = entityDisplays.FirstOrDefault(x => x.ClientId == NetworkManager.Singleton.LocalClientId);

        if(myDisplay != null)
        {
            if(myDisplay.transform.GetSiblingIndex() >= entitiesToDisplay)
            {
                leaderboardEntityHolder.GetChild(entitiesToDisplay - 1).gameObject.SetActive(false);
                myDisplay.gameObject.SetActive(true);
            }
        }
    }


    private void HandlePlayerSpawned(TankPlayer player)
    {
        leaderboardEntities.Add(new LeaderboardEntityState{
            ClientId = player.OwnerClientId,
            PlayerName = player.PlayerName.Value,
            Coins = 0
        });

        player.Wallet.TotalCoins.OnValueChanged += (oldCoins, newCoins) => HandleCoinsChange(player.OwnerClientId, newCoins);
    }

    private void HandlePlayerDespawnd(TankPlayer player)
    {
        if(leaderboardEntities == null){return;}

        foreach(LeaderboardEntityState entity in leaderboardEntities)
        {
            if(entity.ClientId != player.OwnerClientId){continue;}

            leaderboardEntities.Remove(entity);
            break;
        }
        player.Wallet.TotalCoins.OnValueChanged -= (oldCoins, newCoins) => HandleCoinsChange(player.OwnerClientId, newCoins);
    }

    private void HandleCoinsChange(ulong clientId, int newCoins)
    {
        for(int i = 0; i<leaderboardEntities.Count; i++)
        {
            if(leaderboardEntities[i].ClientId != clientId){continue;}
            leaderboardEntities[i] = new LeaderboardEntityState
            {
                ClientId = leaderboardEntities[i].ClientId,
                PlayerName = leaderboardEntities[i].PlayerName,
                Coins = newCoins
            };
            return;
        }
    }
}
