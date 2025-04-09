using Unity.Netcode;
using UnityEngine;

public class PlayerAiming : NetworkBehaviour
{
    [SerializeField] private InputReader inputReader;
    [SerializeField] private Transform turretTransform;
    [SerializeField] private float rotationSpeed = 15f;
 

    private void LateUpdate()
    {
        if(!IsOwner) return; // Ensures only the owner can control the turret

        Aiming();
    
    }

    private void Aiming()
    {
        //Aim position from the input reader
        Vector2 aimScreenPosition = inputReader.AimPosition;

        Ray ray = Camera.main.ScreenPointToRay(aimScreenPosition);
        RaycastHit hit;

       
        if (Physics.Raycast(ray, out hit))
        {
            Vector3 aimWorldPosition = hit.point;


            // Calculate direction vector from turret to the aim position
            Vector3 direction = aimWorldPosition - turretTransform.position;

            direction.y = 0;

            // turretTransform.forward = direction.normalized; 
            Quaternion targetRotation = Quaternion.LookRotation(direction);

            // Smoothly interpolate towards the target rotation
            turretTransform.rotation = Quaternion.Slerp(
                turretTransform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
        }

    }
}
