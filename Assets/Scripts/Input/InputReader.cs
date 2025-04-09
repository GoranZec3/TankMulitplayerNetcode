using System;
using UnityEngine;
using UnityEngine.InputSystem;
using static Controls;


[CreateAssetMenu(fileName = "New Input Reader", menuName = "Input/InputReader", order = 0)]
public class InputReader : ScriptableObject, IPlayerActions
{

    public event Action<bool> PrimaryFireEvent;
    public event Action<Vector3> MoveEvent;
    public event Action<bool> OrbitCameraEvent;

    public Vector2 AimPosition{get; private set;} //can be get from anywhere but set only here

    private Controls controls;

    private void OnEnable()
    {
        if(controls == null)
        {
            controls = new Controls();
            controls.Player.SetCallbacks(this);
        }

        controls.Player.Enable();
    }

    private void OnDisable()
    {
        if (controls != null)
        {
            controls.Player.Disable();
        }
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        MoveEvent?.Invoke(context.ReadValue<Vector3>());         
    }

    public void OnPrimaryFire(InputAction.CallbackContext context)
    {
        if(context.performed)
        {
            PrimaryFireEvent?.Invoke(true);
        }
        else if(context.canceled)
        {
            PrimaryFireEvent?.Invoke(false);
        }
    }

    public void OnAim(InputAction.CallbackContext context)
    {
        AimPosition = context.ReadValue<Vector2>();
    }

    public void OnOrbitCamera(InputAction.CallbackContext context)
    {
        if(context.performed)
        {
            OrbitCameraEvent?.Invoke(true);
        }
        else if(context.canceled)
        {
            OrbitCameraEvent?.Invoke(false);
        }
    }
}
