using UnityEngine;

public class SpawnOnDestroy : MonoBehaviour
{
    [SerializeField] private GameObject prefabExplosion;
    [SerializeField] private float offsetDistance = 1f;

    private void OnDestroy()
    {
        if (prefabExplosion != null)
        {
            if(!gameObject.scene.isLoaded){return;}
            Vector3 spawnPosition = transform.position - transform.forward * offsetDistance;
            Instantiate(prefabExplosion, spawnPosition, Quaternion.identity);          
        }
    }
}
