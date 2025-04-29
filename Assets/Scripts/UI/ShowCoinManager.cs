using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class ShowCoinManager : NetworkBehaviour
{
    [SerializeField] private TMP_Text coinsText;
    
    private CoinWallet localWallet;

    public override void OnNetworkSpawn()
    {
        //Initialize text
        coinsText.text = "Coins: 0";

        FindLocalWallet();

        if (localWallet == null)
        {
            Invoke(nameof(FindLocalWallet), 0.5f);
        }
    }

    private void FindLocalWallet()
    {
        // Get local player object
        var localPlayerObject = NetworkManager.Singleton?.LocalClient?.PlayerObject;
        if (localPlayerObject == null) return;
        

        if (!localPlayerObject.TryGetComponent<TankPlayer>(out var tankPlayer)) return;
        

        localWallet = tankPlayer.Wallet;
        
        if (localWallet != null)
        {
            UpdateCoinsDisplay(localWallet.TotalCoins.Value);
            
            localWallet.TotalCoins.OnValueChanged += OnCoinsChanged;
        }
        else
        {
            Debug.LogWarning("Wallet not found on local player");
        }
    }

    private void OnCoinsChanged(int oldValue, int newValue)
    {
        UpdateCoinsDisplay(newValue);
    }

    private void UpdateCoinsDisplay(int amount)
    {
        coinsText.text = $"Coins: {amount}";
    }

    private void OnDestroy()
    {
        if (localWallet != null)
        {
            localWallet.TotalCoins.OnValueChanged -= OnCoinsChanged;
        }
    }
}
