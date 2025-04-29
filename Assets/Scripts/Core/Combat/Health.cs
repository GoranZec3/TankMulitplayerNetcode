using System;
using Unity.Netcode;
using UnityEngine;

public class Health : NetworkBehaviour
{
    [field: SerializeField] public int MaxHealt {get; private set;} = 100;
    public ulong LastDamageDealer { get; private set; } = ulong.MaxValue;
    [SerializeField] CameraShake cameraShake;
    [SerializeField] AudioSource hitSound;

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
        PlayHitSoundClientRpc(transform.position);
        if (OwnerClientId != NetworkManager.ServerClientId)
        {
            ShakeCameraClientRpc(OwnerClientId);
        }
        else
        {
            hitSound.Play();    
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
            LastDamageDealer = ulong.MaxValue; //reset LastDamageDealer
        }
    }

    public void RegisterDamageDealer(ulong dealerId)
    {
        LastDamageDealer = dealerId;
    }

     [ClientRpc]
    private void ShakeCameraClientRpc(ulong clientId)
    {
        // Only trigger on the intended client
        if (NetworkManager.Singleton.LocalClientId != clientId) return;

        cameraShake.ShakeCameraOnHit();
    }

    [ClientRpc]
    private void PlayHitSoundClientRpc(Vector3 hitPosition)
    {
        AudioSource.PlayClipAtPoint(hitSound.clip, hitPosition);
    }

}
