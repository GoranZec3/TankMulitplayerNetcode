using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;

public class SoundMovement : NetworkBehaviour
{
    public float minSpeed = 0f;
    public float maxSpeed = 20f;
    private float currentSpeed;

    public float minPitch = 0.5f;
    public float maxPitch = 1.2f;
    private float targetPitch;
    private float pitchVelocity; // for SmoothDamp
    public float pitchSmoothTime = 0.2f; // for responsiveness
    [SerializeField] private AudioSource tankAudio;
    [SerializeField] private AudioSource radioCommunication;
    [SerializeField] private Rigidbody tankRb;

    public override void OnNetworkSpawn()
    {
        if (IsClient)
        {
            if (IsOwner)
            {
                tankAudio.enabled = true;
                radioCommunication.enabled = true;
                
                if (SceneManager.GetActiveScene().name == "Gameplay")
                {
                    tankAudio.Play();
                    radioCommunication.Play();
                }
            }
            else
            {
                // Disable audio for other players' tanks
                tankAudio.enabled = false;
                radioCommunication.enabled = false;
            }
        }
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Gameplay" && IsOwner)
        {
            tankAudio.Play();
            radioCommunication.Play();
        }
    }

    // void OnEnable()
    // {
    //     SceneManager.sceneLoaded += OnSceneLoaded;
    // }

    // void OnDisable()
    // {
    //     SceneManager.sceneLoaded -= OnSceneLoaded;
    // }

    // private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    // {
    //     if (scene.name == "Gameplay")
    //     {
    //         tankAudio.Play();
    //         radioCommunication.Play();
    //     }
    // }

    void Update()
    {
        EngineSound();
    }

    void EngineSound()
    {
        currentSpeed = tankRb.linearVelocity.magnitude;

        float speedPercent = Mathf.InverseLerp(minSpeed, maxSpeed, currentSpeed);
        targetPitch = Mathf.Lerp(minPitch, maxPitch, speedPercent);

        // Smooth pitch transition
        tankAudio.pitch = Mathf.SmoothDamp(tankAudio.pitch, targetPitch, ref pitchVelocity, pitchSmoothTime);
    }
}
