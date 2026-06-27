using UnityEngine;

public class SlimeHover : MonoBehaviour
{
    [Header("Hover Settings")]
    public float hoverHeight = 0.8f;
    public float floatAmplitude = 0.2f;
    public float floatSpeed = 2.0f;

    [Header("Rotation Settings")]
    public float rotationSpeed = 100f; // Add this line

    private Terrain terrain;

    void Start()
    {
        terrain = Terrain.activeTerrain;
    }

    void Update()
    {
        if (terrain == null) return;

        // 1. Handle Floating Logic
        float groundY = terrain.SampleHeight(transform.position) + terrain.transform.position.y;
        float bobbing = Mathf.Sin(Time.time * floatSpeed) * floatAmplitude;
        transform.position = new Vector3(transform.position.x, groundY + hoverHeight + bobbing, transform.position.z);

        // 2. Handle Rotation Logic (The "Animation")
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
    }
}