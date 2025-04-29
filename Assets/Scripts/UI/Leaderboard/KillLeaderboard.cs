using Unity.Netcode;
using UnityEngine;



public class KillLeaderboard : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private InputReader inputReader;
    [SerializeField] private RectTransform killLeaderbord;

    public override void OnNetworkSpawn()
    {
        if (inputReader != null)
        {
            inputReader.LeaderboardShow -= HandleLeaderboard; // Prevent duplicates
            inputReader.LeaderboardShow += HandleLeaderboard;
        }
    }

    public override void OnNetworkDespawn()
    {
        if (inputReader != null)
            inputReader.LeaderboardShow -= HandleLeaderboard;
    }

    private void HandleLeaderboard(bool showLeaderboard)
    {
        killLeaderbord.gameObject.SetActive(showLeaderboard);
    }
    
}
