using UnityEngine;

public class SlimePickup : MonoBehaviour
{
    public int slimeValue = 1; // For the score
    public float healAmount = 5f; // For the health

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (AudioManager.instance != null)
                AudioManager.instance.PlaySFX(AudioManager.instance.slimePickup);
            PlayerStats stats = other.GetComponent<PlayerStats>();
            if (stats != null)
            {
                // Pass BOTH values to the player
                stats.AddSlime(slimeValue, healAmount);
                Destroy(gameObject);
            }
        }
    }
}