using UnityEngine;

public class DestroySelfOnContact : MonoBehaviour
{

    private void OnTriggerEnter(Collider other)
    {
        Destroy(gameObject);
        

    }
}
