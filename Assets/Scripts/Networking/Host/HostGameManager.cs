using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Lobbies;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Services.Lobbies.Models;
using System.Collections;
using System.Text;
using Unity.Services.Authentication;


public class HostGameManager : IDisposable
{

    private Allocation allocation;
    private string joinCode;
    private string lobbyId;

    public NetworkServer NetworkServer{get; private set;}

    private const int MaxConnections = 20;

    private const string GameSceneName = "Gameplay";

    
    public async Task StartHostAsync()
    {
        try
        {
            allocation = await RelayService.Instance.CreateAllocationAsync(MaxConnections);
        }
        catch(Exception ex){
            Debug.Log(ex);
            return;
        }    

        try
        {
            joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            Debug.Log(joinCode);
        }
        catch(Exception ex){
            Debug.Log(ex);
            return;
        }  

        UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();

        transport.SetRelayServerData(AllocationUtils.ToRelayServerData(allocation, "dtls"));
        

        try
        {
            CreateLobbyOptions lobbyOptions = new CreateLobbyOptions();
            lobbyOptions.IsPrivate = false;
            lobbyOptions.Data = new Dictionary<string, DataObject>()
            {
                {
                    "JoinCode", new DataObject(
                        visibility: DataObject.VisibilityOptions.Member, 
                        value: joinCode
                        )
                }
            };
            string playerName = PlayerPrefs.GetString(NameSelector.PlayerNameKey, "Unknown");
            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync($"{playerName}'s Lobby", MaxConnections, lobbyOptions);
            lobbyId = lobby.Id;

            HostSingleton.Instance.StartCoroutine(HeartbeatLobby(15));
        }
        catch(LobbyServiceException e)
        {
            Debug.Log(e);
            return;
        }

        NetworkServer = new NetworkServer(NetworkManager.Singleton);

        UserData userData = new UserData
        {
            userName = PlayerPrefs.GetString(NameSelector.PlayerNameKey, "Missing Name"),
            userAuthId = AuthenticationService.Instance.PlayerId
        };

        string payload = JsonUtility.ToJson(userData);
        byte[] payloadBytes = Encoding.UTF8.GetBytes(payload);

        NetworkManager.Singleton.NetworkConfig.ConnectionData = payloadBytes;

        NetworkManager.Singleton.StartHost();

        NetworkServer.OnClientLeft += HandleClientLeft;


        NetworkManager.Singleton.SceneManager.LoadScene(GameSceneName, LoadSceneMode.Single);
    }
    //keep alive lobby
    private IEnumerator HeartbeatLobby(float waitTimeSeconds)
    {
        WaitForSecondsRealtime delay = new WaitForSecondsRealtime(waitTimeSeconds); 
        while(true)
        {
            LobbyService.Instance.SendHeartbeatPingAsync(lobbyId);
            yield return delay;
        }
    }

    //Called if server shutdown unexpectedly 
    public void Dispose()
    {
        Shutdown();
    }

    //called on button in gameHUD
    public async void Shutdown()
    {

        if (string.IsNullOrEmpty(lobbyId)){return;}
        
        
        HostSingleton.Instance.StopCoroutine(nameof(HeartbeatLobby));
        try
        {
            await LobbyService.Instance.DeleteLobbyAsync(lobbyId);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }

        lobbyId = string.Empty;
        
        NetworkServer.OnClientLeft -= HandleClientLeft;

        NetworkServer?.Dispose();
    }

    private async void HandleClientLeft(string authId)
    {

        if (string.IsNullOrEmpty(lobbyId))
        {
            Debug.LogWarning("HandleClientLeft called, but lobbyId is null or empty.");
            return;
        }

        try
        {
            await LobbyService.Instance.RemovePlayerAsync(lobbyId, authId);
        }
        catch (LobbyServiceException e)
        {
            if (e.Reason == LobbyExceptionReason.LobbyNotFound)
            {
                Debug.LogWarning($"Lobby not found while trying to remove player {authId}. Likely already deleted.");
            }
            else
            {
                Debug.LogError($"Failed to remove player {authId} from lobby: {e.Message}");
            }
        }
    }
}
