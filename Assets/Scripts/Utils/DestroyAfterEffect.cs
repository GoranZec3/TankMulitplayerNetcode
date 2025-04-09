using UnityEngine;

public class DestroyAfterEffect : MonoBehaviour
{
    private ParticleSystem particleSys;

    [SerializeField] GameObject targetToDestory = null;
    // Update is called once per frame
    void Update()
    {

        if(!GetComponent<ParticleSystem>().IsAlive())
        {
            
            //targetToDestory would be empty as parent
            if(targetToDestory != null)
            {
                Destroy(targetToDestory);
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
    }
    
}
