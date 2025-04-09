using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class ParticleAligner : MonoBehaviour
{
    private ParticleSystem.MainModule psMain;

    void Start()
    {
        psMain = GetComponent<ParticleSystem>().main;
    }


    void Update()
    {
        // psMain.startRotation = -transform.rotation.eulerAngles.y * Mathf.Deg2Rad;
        float angle = Mathf.Atan2(transform.forward.x, transform.forward.z);
        psMain.startRotation = angle;
    }
}
