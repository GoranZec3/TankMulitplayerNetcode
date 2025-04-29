using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(Health))]  // Ensures Health component exists
public class KillTracker : NetworkBehaviour
{
    [Header("References")]
    [field: SerializeField] public Health Health { get; private set; }
    public NetworkVariable<int> TotalKills = new NetworkVariable<int>();

    private void Awake()
    {
        // Auto-assign Health reference if not set in inspector
        if (Health == null) 
        {
            Health = GetComponent<Health>();
            if (Health == null)
            {
                Debug.LogError("Health component missing!", this);
                return;
            }
        }
    }

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;
        
        Health.OnDie += HandleKill;
    }

    public override void OnNetworkDespawn()
    {
        if (!IsServer) return;
        
        Health.OnDie -= HandleKill;
    }

    private void HandleKill(Health deadHealth)
    {
        ulong killerId = deadHealth.LastDamageDealer;

        // Skip invalid kills
        if (killerId == ulong.MaxValue || killerId == deadHealth.OwnerClientId) 
            return;

        // Find killer and increment their kills
        foreach (var player in FindObjectsByType<NetworkObject>(FindObjectsSortMode.None))
        {
            if (player.OwnerClientId == killerId &&
                player.TryGetComponent<KillTracker>(out var killerTracker))
            {
                killerTracker.AddKill();
                break;
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void AddKillServerRpc()
    {
        if (!IsServer) return;
        TotalKills.Value += 1;
    }

    public void AddKill()
    {
        if (!IsServer) return;
        TotalKills.Value += 1;
    }
}