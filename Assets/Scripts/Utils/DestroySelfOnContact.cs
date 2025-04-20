using UnityEngine;

public class DestroySelfOnContact : MonoBehaviour
{
    [SerializeField] private AudioSource explosionSound;

    private void OnTriggerEnter(Collider other)
    {
        if (explosionSound != null)
        {
            // Detach sound from projectile
            explosionSound.transform.parent = null;
            explosionSound.Play();

            // Destroy the audio after it finishes playing
            Destroy(explosionSound.gameObject, explosionSound.clip.length);
        }
        Destroy(gameObject);
    }
}
