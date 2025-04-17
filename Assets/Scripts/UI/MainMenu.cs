using TMPro;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
    
    [SerializeField] private TMP_InputField joinCodeField;

    private bool isBusy;



    void Start()
    {
        if(ClientSingleton.Instance == null){return;}
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }

    public async void StartHost()
    {
        if(isBusy){return;}
        isBusy = true;
        await HostSingleton.Instance.GameManager.StartHostAsync();
        isBusy = false;
    }

    public async void StartTeamHost()
    {
        if(isBusy){return;}
        isBusy = true;
        Debug.Log("Start team host");
        HostSingleton.Instance.GameManager.SetMatchMode(true);
        await HostSingleton.Instance.GameManager.StartHostAsync();
        isBusy = false;
    }


    public async void StartClient()
    {
        if(isBusy){return;}
        isBusy = true;
        await ClientSingleton.Instance.GameManager.StartClientAsync(joinCodeField.text);
        isBusy = false;
    }

    public async void JoinAsync(Lobby lobby, int teamId)
    {

        if(isBusy){return;}
        isBusy = true;

        try{
            Lobby joiningLobby = await LobbyService.Instance.JoinLobbyByIdAsync(lobby.Id);
            string joinCode = joiningLobby.Data["JoinCode"].Value;

            //should set teamId
            ClientSingleton.Instance.GameManager.SetTeamId(teamId);

            await ClientSingleton.Instance.GameManager.StartClientAsync(joinCode);
        }
        catch(LobbyServiceException e)
        {
            Debug.Log(e);
        }

        isBusy = false;
    }
}
