using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class LobbyItem : MonoBehaviour
{
    [SerializeField] private GameObject joinButton;
    [SerializeField] private GameObject teamAButton;
    [SerializeField] private GameObject teamBButton;
    
 
    [SerializeField] private TMP_Text lobbyNameText;
    [SerializeField] private TMP_Text lobbyPlayersText;

    private LobbiesList lobbiesList;
    private Lobby lobby;

    // public void Initialise(LobbiesList lobbiesList, Lobby lobby)
    // {
    //     this.lobbiesList = lobbiesList;
    //     this.lobby = lobby;

    //     lobbyNameText.text = lobby.Name;
    //     lobbyPlayersText.text = $"{lobby.Players.Count}/{lobby.MaxPlayers}";
    // }

    public void Initialise(LobbiesList lobbiesList, Lobby lobby)
    {
        this.lobbiesList = lobbiesList;
        this.lobby = lobby;

        lobbyNameText.text = lobby.Name;
        lobbyPlayersText.text = $"{lobby.Players.Count}/{lobby.MaxPlayers}";

        bool isTeamMatch = lobby.Data != null && 
                        lobby.Data.ContainsKey("TeamMatch") && 
                        lobby.Data["TeamMatch"].Value == "true";

        joinButton.SetActive(!isTeamMatch);
        teamAButton.SetActive(isTeamMatch);
        teamBButton.SetActive(isTeamMatch);
    }
    
    public void Join()
    {
        lobbiesList.JoinAsync(lobby, -1);
    }

    public void JoinTeamA()
    {
        lobbiesList.JoinAsync(lobby, 0); // 0 = Team A
    }

    public void JoinTeamB()
    {
        lobbiesList.JoinAsync(lobby, 1); // 1 = Team B
    }
}
