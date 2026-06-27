using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    [Header("Checkpoint Locations")]
    // Array of empty GameObjects marking where the player should stand
    public Transform[] checkpointLocations;

    void Start()
    {
        // Safety check to avoid out-of-bounds errors
        if (GameSettings.LastCheckpointIndex < checkpointLocations.Length)
        {
            // Teleport the player
            transform.position = checkpointLocations[GameSettings.LastCheckpointIndex].position;
            transform.rotation = checkpointLocations[GameSettings.LastCheckpointIndex].rotation;
        }
    }
}