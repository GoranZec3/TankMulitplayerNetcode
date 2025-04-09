using System;
using Unity.Cinemachine;
using UnityEngine;


[RequireComponent(typeof(CinemachineInputAxisController))]
public class CameraLook : MonoBehaviour
{
    [SerializeField] private InputReader inputReader;
    private CinemachineInputAxisController inputController;

    
    private void Awake()
    {
        inputController = GetComponent<CinemachineInputAxisController>();
        inputController.enabled = false;
    }

    private void OnEnable()
    {
        inputReader.OrbitCameraEvent += HandleOrbitCamera;
    }

    private void OnDisable()
    {
        inputReader.OrbitCameraEvent -= HandleOrbitCamera;
    }

    private void HandleOrbitCamera(bool pressed)
    {
        inputController.enabled = pressed;
    }
}
