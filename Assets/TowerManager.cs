using UnityEngine;
using StarterAssets;

public class TowerManager : MonoBehaviour
{
    public Tower[] towers;
    public int currentTowerIndex = 0;

    // GLOBAL DIFFICULTY: Higher = Enemies hit harder and take more hits to kill.
    // We start at 1.0. Every tower finished makes this go up.
    public static float globalDifficulty = 1.0f;

    void Awake()
    {
        // 1. Sync the TowerManager with the saved checkpoint!
        currentTowerIndex = GameSettings.LastCheckpointIndex;

        // 2. Fast-forward the global difficulty (1.0 + 15% per completed tower)
        globalDifficulty = 1.0f + (0.15f * currentTowerIndex);
    }

    void Start()
    {
        // 3. Give the player their upgrades back based on the checkpoint
        RestoreUpgradesOnLoad();

        // 4. Update the towers (This automatically makes past towers green!)
        RefreshTowers();
    }

    public void TowerActivated()
    {
        // 1. Grant Reward for the tower just completed (before moving the index)
        GrantTowerReward(currentTowerIndex);

        // 2. Increase Global Difficulty
        // We add 0.15 (15%) per tower. 
        // This is why they take 3 hits to kill after Tower 1!
        globalDifficulty += 0.15f;
        Debug.Log("Tower " + (currentTowerIndex + 1) + " Restored! Difficulty: " + globalDifficulty);

        // 3. Move to the next tower
        currentTowerIndex++;

        // --- NEW CHECKPOINT LOGIC ---
        // Save our progress to the static data bridge
        GameSettings.LastCheckpointIndex = currentTowerIndex;
        Debug.Log("CHECKPOINT SAVED: " + GameSettings.LastCheckpointIndex);
        // ----------------------------

        // 4. Update all towers so the next one turns Red and the old ones stay Green
        RefreshTowers();

        // 5. Check if that was the last tower
        if (currentTowerIndex >= towers.Length)
        {
            TriggerWin();
        }
    }

    void GrantTowerReward(int index)
    {
        // Use the Namespace if needed
        StarterAssets.FirstPersonController movement = Object.FindFirstObjectByType<StarterAssets.FirstPersonController>();

        if (movement != null)
        {
            if (AudioManager.instance != null)
                AudioManager.instance.PlaySFX(AudioManager.instance.towerActivation);

            Debug.Log("Found Player! Granting reward for Tower: " + index);

            // We use a switch to handle multiple towers easily
            switch (index)
            {
                case 0: // Tower 1 (Index 0)
                    movement.canGlide = true;
                    movement.MoveSpeed = 8f;
                    Debug.Log("REWARD: Glide and Speed Boost Unlocked!");
                    break;

                case 1: // Tower 2 (Index 1) - THE NEW CODE GOES HERE
                    movement.canShoot = true;
                    // Let's give a small speed boost too so it feels rewarding
                    movement.MoveSpeed *= 1.15f;
                    Debug.Log("REWARD: Laser Pulse Rifle Unlocked!");
                    break;

                case 2: // Tower 3 (Placeholder for later)
                    Debug.Log("REWARD: Tower 3 logic ready for coding!");
                    break;
            }
        }
        else
        {
            Debug.LogError("CRITICAL: TowerManager could not find FirstPersonController!");
        }
    }

    public void RefreshTowers()
    {
        for (int i = 0; i < towers.Length; i++)
        {
            if (towers[i] != null)
            {
                if (i < currentTowerIndex)
                {
                    // PAST TOWERS: Already activated! Turn the beam back on.
                    towers[i].SetCompleted();
                }
                else if (i == currentTowerIndex)
                {
                    // PRESENT TOWER: This is the current objective (Turn Red)
                    towers[i].SetObjective(true);
                }
                else
                {
                    // FUTURE TOWERS: Not ready yet.
                    towers[i].SetObjective(false);
                }
            }
        }
    }

    void TriggerWin()
    {
        PlayerStats stats = Object.FindFirstObjectByType<PlayerStats>();
        if (stats != null && stats.winPanel != null)
        {
            stats.winPanel.SetActive(true);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    // --- NEW FUNCTION: Silently restores mechanics ---
    void RestoreUpgradesOnLoad()
    {
        StarterAssets.FirstPersonController movement = Object.FindFirstObjectByType<StarterAssets.FirstPersonController>();
        if (movement != null)
        {
            // If they have passed Tower 1 (Index 0)
            if (currentTowerIndex > 0)
            {
                movement.canGlide = true;
                movement.MoveSpeed = 8f;
            }

            // If they have passed Tower 2 (Index 1)
            if (currentTowerIndex > 1)
            {
                movement.canShoot = true;
                movement.MoveSpeed = 8f * 1.15f;
            }

            // Tower 3 (Index 2) logic goes here when you build it!
        }
    }

}