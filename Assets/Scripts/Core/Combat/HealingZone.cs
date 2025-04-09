using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine.UI;
using UnityEngine;

public class HealingZone : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private Image healPowerBar;

    [Header("Settings")]
    [SerializeField] private int maxHealPower= 30;
    [SerializeField] private float healCooldown = 60f;
    [SerializeField] private float healTickRate = 1f;
    [SerializeField] private int coindPerTick = 10;
    [SerializeField] private int healthPerTick = 10;

    private float remainingCooldown;
    private float tickTimer;

    private List<TankPlayer> playersInZone = new List<TankPlayer>();

    private NetworkVariable<int> HealPower = new NetworkVariable<int>();

    public override void OnNetworkSpawn()
    {
        if(IsClient)
        {
            HealPower.OnValueChanged += HandleHealPowerChanged;
            HandleHealPowerChanged(0, HealPower.Value);
        }

        if(IsServer)
        {
            //reseting heal to 100% at start
            HealPower.Value = maxHealPower;
        }
    }

    public override void OnNetworkDespawn()
    {
        if(IsClient)
        {
            HealPower.OnValueChanged -= HandleHealPowerChanged;
        }
        
    }

    void OnTriggerEnter(Collider collider)
    {
        if(!IsServer){return;}

        if(!collider.attachedRigidbody.TryGetComponent<TankPlayer>(out TankPlayer player)){return;}
        playersInZone.Add(player);


    }

    void OnTriggerExit(Collider collider)
    {
        if(!IsServer){return;}
        if(!collider.attachedRigidbody.TryGetComponent<TankPlayer>(out TankPlayer player)){return;}
        playersInZone.Remove(player);


    }

    private void Update()
    {
        if(!IsServer) {return;}

        //cooldown logic
        if(remainingCooldown > 0)
        {
            remainingCooldown -= Time.deltaTime;

            if(remainingCooldown <= 0)
            {
                HealPower.Value = maxHealPower;
            }
            else
            {
                return;
            }
        }

        tickTimer += Time.deltaTime;
        if(tickTimer >= 1/healTickRate)
        {
            foreach(TankPlayer player in playersInZone)
            {
                if(HealPower.Value == 0){break;}
                //skip full health players
                if(player.Health.CurrentHelth.Value == player.Health.MaxHealt){continue;}
                //skip poor players
                if(player.Wallet.TotalCoins.Value < coindPerTick){continue;}

                //heal and payment
                player.Wallet.SpendCoins(coindPerTick);
                player.Health.RestoreHealt(healthPerTick);

                HealPower.Value -= 1;

                //no more healing, wait
                if(HealPower.Value == 0)
                { 
                    remainingCooldown = healCooldown;
                }
            }

            tickTimer = tickTimer % (1/healTickRate);
        }
    }


    private void HandleHealPowerChanged(int oldHealPower, int newHealPower)
    {
        healPowerBar.fillAmount = (float)newHealPower / maxHealPower;
    }
    
}
