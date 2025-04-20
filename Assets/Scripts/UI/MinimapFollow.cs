using UnityEngine;

public class MinimapFollow : MonoBehaviour
{
    [SerializeField] private Transform playerTransform;

    void LateUpdate()
    {
        // Vector3 newPosition = playerTransform.position;
        // newPosition.y = transform.position.y; 
        // transform.position = newPosition;

        transform.rotation = Quaternion.Euler(90f, playerTransform.eulerAngles.y, 0f); 
    }
}
