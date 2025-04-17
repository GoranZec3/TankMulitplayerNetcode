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
using Unity.Cinemachine;


public class HostGameManager : IDisposable
{

    private Allocation allocation;
    private string joinCode;
    private string lobbyId;

    public NetworkServer NetworkServer{get; private set;}

    private const int MaxConnections = 20;

    private const string GameSceneName = "Gameplay";

    private bool isTeamMatchMode = false;



    public void SetMatchMode(bool isTeamMatch)
    {
        isTeamMatchMode = isTeamMatch;
    }


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
                }, 
                { "TeamMatch", new DataObject(
                    visibility: DataObject.VisibilityOptions.Public, 
                    value: isTeamMatchMode.ToString().ToLower()    
                )}
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

        //Filling up user data 
        UserData userData = new UserData
        {
            userName = PlayerPrefs.GetString(NameSelector.PlayerNameKey, "Missing Name"),
            userAuthId = AuthenticationService.Instance.PlayerId,
            teamId = isTeamMatchMode ? 0 : -1
        };
        Debug.Log(userData.teamId);

        string payload = JsonUtility.ToJson(userData);
        byte[] payloadBytes = Encoding.UTF8.GetBytes(payload);

        NetworkManager.Singleton.NetworkConfig.ConnectionData = payloadBytes;


        NetworkManager.Singleton.StartHost();

        
        SetHostSpawnPosition();

        NetworkServer.OnClientLeft += HandleClientLeft;

        NetworkManager.Singleton.SceneManager.LoadScene(GameSceneName, LoadSceneMode.Single);
    }

    private async void SetHostSpawnPosition()
    {

        while (SceneManager.GetActiveScene().name != GameSceneName)
        {
            await Task.Delay(500); // Wait for the scene to be fully loaded
        }
        UserData hostData = HostSingleton.Instance.GameManager.NetworkServer.GetUserDataByClientId(NetworkManager.Singleton.LocalClient.ClientId);
        Team hostTeam = (Team)hostData.teamId;
        var hostSpawnPosition = SpawnPoint.GetRandomSpawnPoint(hostTeam);
        
        // Vector3 hostSpawnPosition = SpawnPoint.GetRandomSpawnPos();
        // var hostSpawnPosition = SpawnPoint.GetRandomSpawnPoint();
   
        if (hostSpawnPosition.position == Vector3.zero)
        {
            Debug.LogWarning("No spawn points available. Using default position.");
            // hostSpawnPosition = new Vector3(0f, 0f, 0f);
        }

        if (NetworkManager.Singleton.LocalClient.PlayerObject != null)
        {
            //initial position
            var playerTransform = NetworkManager.Singleton.LocalClient.PlayerObject.transform;
            playerTransform.position = hostSpawnPosition.position; 

            // Face toward center of the map
            Vector3 directionToCenter = (Vector3.zero - hostSpawnPosition.position).normalized;
            playerTransform.rotation = Quaternion.LookRotation(directionToCenter);

            //Face camera...
            var orbitalFollow = playerTransform.GetComponentInChildren<CinemachineOrbitalFollow>(); // Get the camera
            if (orbitalFollow != null)
            {       
                float rotationY = playerTransform.rotation.eulerAngles.y;
                // Normalize it to the 0-360 range (it should already be in this range)
                if (rotationY < 0) rotationY += 360;
                orbitalFollow.HorizontalAxis.Value = rotationY;
            }
        }
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
