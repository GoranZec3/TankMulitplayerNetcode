using UnityEngine;

public class DestroyExplosion : MonoBehaviour
{
    [SerializeField] private ParticleSystem particleSystemToWatch;

    private void LateUpdate()
    {
        if (particleSystemToWatch != null && !particleSystemToWatch.IsAlive())
        {
            Destroy(gameObject);
        }
    }
    
}
