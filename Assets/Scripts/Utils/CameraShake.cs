using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    [SerializeField] CinemachineRotationComposer rotationComposer;
    [SerializeField] private float shakeIntensity = 2f;
    [SerializeField] private float shakeDuration = 0.07f;
    [SerializeField] private float shakeSpeed = 30f;

    private bool isShaking;
    private Vector3 originalTrackedOffset;

     private void Awake()
    {
        if (rotationComposer != null)
        {
            originalTrackedOffset = rotationComposer.TargetOffset;
        }
    }

    public void ShakeCameraOnHit()
    {
        
        if (!isShaking && rotationComposer != null)
        {
            StartCoroutine(ShakeCoroutine());
        } 
    }

    private IEnumerator ShakeCoroutine()
    {
        isShaking = true;
        float timer = 0f;

        while (timer < shakeDuration)
        {
            timer += Time.deltaTime;

            // Apply random small offsets
            Vector3 randomOffset = new Vector3(
                Random.Range(-shakeIntensity, shakeIntensity),
                Random.Range(-shakeIntensity, shakeIntensity),
                0f); // Don't shake forward/back

            rotationComposer.TargetOffset = originalTrackedOffset + randomOffset;

            yield return null;
        }

        // Smoothly transition back to original offset
        float transitionSpeed = shakeSpeed;  // How fast to return to the original position
        while (Vector3.Distance(rotationComposer.TargetOffset, originalTrackedOffset) > 0.01f)
        {
            rotationComposer.TargetOffset = Vector3.Lerp(rotationComposer.TargetOffset, originalTrackedOffset, transitionSpeed * Time.deltaTime);
            yield return null;
        }

        // Ensure we arrive at the original position exactly
        rotationComposer.TargetOffset = originalTrackedOffset;

        isShaking = false;
    }
}
