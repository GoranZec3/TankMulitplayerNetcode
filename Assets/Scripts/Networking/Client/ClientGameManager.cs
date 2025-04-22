using System;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Unity.Cinemachine;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ClientGameManager : IDisposable
{
    private JoinAllocation allocation;

    private NetworkClient networkClient;
    private UserData userData;

    private const string GameSceneName = "Gameplay";


    private const string SelectorSceneName = "Selector";

    public async Task<bool> InitAsync()
    {
        await UnityServices.InitializeAsync();

        networkClient = new NetworkClient(NetworkManager.Singleton);

        AuthState authState = await AuthenticationWrapper.DoAuth();

        if(authState == AuthState.Authenticated)
        {
            userData = new UserData
            {
                userName = PlayerPrefs.GetString(NameSelector.PlayerNameKey, "Missing Name"),
                userAuthId = AuthenticationService.Instance.PlayerId,
                tankIndex = PlayerPrefs.GetInt(CharacterSelection.TankIndexKey)
            };
            return true;
        }

        return false;
    }

    public void GoToSelector()
    {
        if (NetworkManager.Singleton.IsClient || NetworkManager.Singleton.IsHost)
        {
            NetworkManager.Singleton.Shutdown();
        }
        SceneManager.LoadScene(SelectorSceneName);
    }
    
    public async Task StartClientAsync(string joinCode)
    {
        try
        {
            allocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
        }
        catch(Exception e)
        {
            Debug.Log(e);
            return;
        }

        UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();  
        transport.SetRelayServerData(AllocationUtils.ToRelayServerData(allocation, "dtls"));

        //SELECTED TANK PREFAB ADD ON RECONNECTION
        userData.tankIndex = PlayerPrefs.GetInt(CharacterSelection.TankIndexKey);

        ConnectClient();

        //initial client rotation 
        SetInitialClientRotation();
    }

    private void ConnectClient()
    {
        string payload = JsonUtility.ToJson(userData);
        byte[] payloadBytes = Encoding.UTF8.GetBytes(payload);
        NetworkManager.Singleton.NetworkConfig.ConnectionData = payloadBytes;

        NetworkManager.Singleton.StartClient();    
    }

    public void SetTeamId(int id)
    {
        userData.teamId = id;
    }



    public void Disconnect()
    {
        if (NetworkManager.Singleton.IsClient || NetworkManager.Singleton.IsHost)
        {
            NetworkManager.Singleton.Shutdown();

        }
        networkClient.Disconnect();
    }

    public void Dispose()
    {
        networkClient?.Dispose();
    }


    private async void SetInitialClientRotation()
    {

        while (SceneManager.GetActiveScene().name != GameSceneName)
        {
            await Task.Delay(250); // Wait for the scene to be fully loaded
        }
        
        var playerTransform = NetworkManager.Singleton.LocalClient.PlayerObject.transform;

        // Set the rotation to face the center of the map (or any other direction you want)
        Vector3 directionToCenter = (Vector3.zero - playerTransform.position).normalized;
        playerTransform.rotation = Quaternion.LookRotation(directionToCenter);

        float rotationY = playerTransform.transform.eulerAngles.y;
        if (rotationY < 0)
        {
            rotationY += 360;
        }     
        var orbitalFollow = playerTransform.GetComponentInChildren<CinemachineOrbitalFollow>();
        // Align orbital camera with player's rotation
        orbitalFollow.HorizontalAxis.Value = rotationY;      
    }

}
