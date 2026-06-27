using UnityEngine;

public class HealthBarFacing : MonoBehaviour
{
    private Transform mainCamera;

    void Start()
    {
        mainCamera = Camera.main.transform;
    }

    // LateUpdate runs after movement, preventing "jittery" UI
    void LateUpdate()
    {
        // Force the UI to always look at the camera
        transform.LookAt(transform.position + mainCamera.forward);
    }
}