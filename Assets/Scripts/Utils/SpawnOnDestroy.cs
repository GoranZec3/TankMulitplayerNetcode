using UnityEngine;
using UnityEngine.SceneManagement;

public class SpawnOnDestroy : MonoBehaviour
{
    [SerializeField] private GameObject prefabExplosion;
    [SerializeField] private float offsetDistance = 1f;

    private void OnDestroy()
    {
        // Safety check because if you fire projectile and leave game, menu freeze
        // if (!gameObject.scene.isLoaded || SceneManager.GetActiveScene().name != "Gameplay")
        //     return;

        if (prefabExplosion != null)
        {
            if(!gameObject.scene.isLoaded){return;}
            Vector3 spawnPosition = transform.position - transform.forward * offsetDistance;
            Instantiate(prefabExplosion, spawnPosition, Quaternion.identity);          
        }
    }
}
