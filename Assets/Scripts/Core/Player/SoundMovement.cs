using UnityEngine;

public class SoundMovement : MonoBehaviour
{
    public float minSpeed = 0f;
    public float maxSpeed = 20f;
    private float currentSpeed;

    private Rigidbody tankRb;
    private AudioSource tankAudio;

    public float minPitch = 0.5f;
    public float maxPitch = 1.2f;
    private float targetPitch;
    private float pitchVelocity; // for SmoothDamp
    public float pitchSmoothTime = 0.2f; // for responsiveness

    void Start()
    {
        tankAudio = GetComponent<AudioSource>();
        tankRb = GetComponent<Rigidbody>();
    }

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
