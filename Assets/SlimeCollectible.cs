using UnityEngine;

public class SlimeCollectible : MonoBehaviour
{
    public int value = 1;
    public float rotationSpeed = 100f;
    public float floatSpeed = 2f;
    public float floatHeight = 0.2f;

    private Vector3 startPos;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        // Gentle rotation
        transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime);

        // Gentle floating/bobbing
        float newY = startPos.y + Mathf.Sin(Time.time * floatSpeed) * floatHeight;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }

    private void OnTriggerEnter(Collider other)
    {
        // Check if the object touching the slime is tagged "Player"
        if (other.CompareTag("Player"))
        {
            PlayerStats stats = other.GetComponent<PlayerStats>();

            if (stats != null)
            {
                stats.AddSlime(value); // This name MUST match the function in PlayerStats
                Destroy(gameObject);
            }
        }
    }
}