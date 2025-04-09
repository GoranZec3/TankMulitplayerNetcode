using Unity.Netcode;
using UnityEngine;

public class HealthDisplay : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private Health health;
    [SerializeField] private RectTransform healthBarImage;

    protected override void OnNetworkPostSpawn()
    {
        if(!IsClient){return;}
        health.CurrentHelth.OnValueChanged += HandleHealthChange;
        HandleHealthChange(0, health.CurrentHelth.Value);
    }

    public override void OnNetworkDespawn()
    {
        if(!IsClient){return;}
        health.CurrentHelth.OnValueChanged -= HandleHealthChange;
    }

    private void HandleHealthChange(int oldHealth, int newHealth)
    {
        
        healthBarImage.localScale = new Vector3((float)newHealth/health.MaxHealt, 1, 1);
    }
}
