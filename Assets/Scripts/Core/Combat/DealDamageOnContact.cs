using Unity.Netcode;
using UnityEngine;

public class DealDamageOnContact : MonoBehaviour
{
    [SerializeField] private int damage = 5;

    private ulong ownerClientId;

    public void SetOwner(ulong ownerClientId)
    {
        this.ownerClientId = ownerClientId;
    }

    private void OnTriggerEnter(Collider collider)
    {

        
        if(collider.attachedRigidbody == null){return;}

        if(collider.attachedRigidbody.TryGetComponent<NetworkObject>(out NetworkObject netObj))
        {
            if(ownerClientId == netObj.OwnerClientId){return;}
        }

        if(collider.attachedRigidbody.TryGetComponent<Health>(out Health health))
        {
            health.TakeDamage(damage);
        }
    }
}
