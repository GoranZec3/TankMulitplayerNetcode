using System;
using Unity.Netcode;
using UnityEngine;

public class Health : NetworkBehaviour
{
    [field: SerializeField] public int MaxHealt {get; private set;} = 100;
    //var witch can only be modified on server
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

}
