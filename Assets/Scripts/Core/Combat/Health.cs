using System;
using Unity.Netcode;
using UnityEngine;

public class Health : NetworkBehaviour
{
    [field: SerializeField] public int MaxHealt {get; private set;} = 100;
    [SerializeField] CameraShake cameraShake;

    //var which can only be modified on server
    public NetworkVariable<int> CurrentHelth = new NetworkVariable<int>();
    private bool isDead;
    public Action<Health> OnDie;

    protected override void OnNetworkPostSpawn()
    {
        if(!IsServer) {return;}

        CurrentHelth.Value = MaxHealt;
    }

    public void TakeDamage(int damageValue)
    {
        ModifyHealth(-damageValue);
        if (OwnerClientId != NetworkManager.ServerClientId)
        {
            ShakeCameraClientRpc(OwnerClientId);
        }
        else
        {
            cameraShake.ShakeCameraOnHit(); // Local (host) player
        }
    }

    public void RestoreHealt(int healValue)
    {
        ModifyHealth(healValue);
    }

    private  void ModifyHealth(int value)
    {
        if(isDead){return;}

        int newHealth = CurrentHelth.Value + value;
        CurrentHelth.Value = Mathf.Clamp(newHealth, 0, MaxHealt);
        

        if(CurrentHelth.Value == 0)
        {
            OnDie.Invoke(this);
            isDead = true;
        }
    }

     [ClientRpc]
    private void ShakeCameraClientRpc(ulong clientId)
    {
        // Only trigger on the intended client
        if (NetworkManager.Singleton.LocalClientId != clientId) return;

        cameraShake.ShakeCameraOnHit();
    }

}
