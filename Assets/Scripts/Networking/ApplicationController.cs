using System.Threading.Tasks;
using UnityEngine;

public class ApplicationController : MonoBehaviour
{

    [SerializeField] private ClientSingleton clientPrefab;
    [SerializeField] private HostSingleton hostPrefab;

    
    private async Task Start()
    {
        DontDestroyOnLoad(this);

        await LaunchInMode(SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null);      
    }

    private async Task LaunchInMode(bool isDedicatedServer)
    {
        if(isDedicatedServer)
        {
            
        }
        else
        {
            HostSingleton hostSingleton = Instantiate(hostPrefab);
            hostSingleton.CreateHost();
            DontDestroyOnLoad(hostSingleton);
           
            ClientSingleton clientSingleton = Instantiate(clientPrefab);
            bool authenticated = await clientSingleton.CreateClient();



            if(authenticated)
            {
                clientSingleton.GameManager.GoToMenu();
            }
        }
    }



}
