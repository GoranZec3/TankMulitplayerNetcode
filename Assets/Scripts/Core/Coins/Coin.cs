using Unity.Netcode;
using UnityEngine;

public abstract class Coin : NetworkBehaviour
{
    [SerializeField] private GameObject coinMesh;

    protected int coinValue = 10;
    protected bool alreadyCollected;

    public abstract int Collect();

    public void SetValue(int value)
    {
        coinValue = value;
    }

    protected void Show(bool show)
    {
        GetComponent<Collider>().enabled = show;
        gameObject.SetActive(show);
        // transform.GetChild(0).gameObject.SetActive(show);
    }
}
