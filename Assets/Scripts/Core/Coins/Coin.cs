using Unity.Netcode;
using UnityEngine;

public abstract class Coin : NetworkBehaviour
{
    [SerializeField] private GameObject coinMesh;

    protected int coinValue = 10;
    protected bool alreadyCollected;

    public abstract int Collect();

    public override void OnNetworkSpawn()
    {
        Show(true); 
    }

    public void SetValue(int value)
    {
        coinValue = value;
    }

    protected void Show(bool show)
    {
        // GetComponent<Collider>().enabled = show;
        // gameObject.SetActive(show);
        GetComponent<Collider>().enabled = show;

        if (coinMesh != null)
            coinMesh.SetActive(show);
    }
}
