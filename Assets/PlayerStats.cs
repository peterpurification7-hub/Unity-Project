using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class PlayerStats : MonoBehaviour
{
    [Header("Game Stats")]
    public float health = 100f;
    public int slimesSaved = 0;

    // NOTE: Vector3 lastCheckpointPos was removed because 
    // PlayerSpawner.cs and GameSettings handle this now!

    [Header("UI Panels")]
    public GameObject gameOverPanel;
    public GameObject winPanel;
    public TextMeshProUGUI popUpText;

    [Header("UI Text")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI healthText;

    void Start()
    {
        // 🖱️ MOUSE FIX: Lock and hide the cursor when the game scene loads
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        UpdateUI();
    }

    public void AddSlime(int amount, float heal = 0f)
    {
        slimesSaved += amount;

        // If heal is 0 (like from the Tower), nothing happens to health
        health = Mathf.Min(100f, health + heal);

        UpdateUI();
    }

    public void UpdateUI()
    {
        if (scoreText != null) scoreText.text = "Slimes: " + slimesSaved;
        if (healthText != null) healthText.text = "Health: " + Mathf.Max(0, (int)health);
    }

    void Update()
    {
        if (health <= 0 && !gameOverPanel.activeSelf) ShowGameOver();
    }

    void ShowGameOver()
    {
        gameOverPanel.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void HidePopUp()
    {
        if (popUpText != null)
        {
            popUpText.transform.parent.gameObject.SetActive(false);
        }
    }

    // --- NEW UNIFIED RELOAD BUTTON ---
    public void ReloadFromCheckpoint()
    {
        // This completely resets the scene (enemies, items, health)
        // The moment it loads, PlayerSpawner.cs will read GameSettings
        // and teleport you to the correct tower instantly!
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void GoToMainMenu()
    {
        SceneManager.LoadScene("MainMenuScene");
    }

    // --- NEW: Hook this up to your "Play Again" button on the Win Panel ---
    public void RestartFullGame()
    {
        // 1. Wipe the static memory clean
        GameSettings.LastCheckpointIndex = 0;

        // 2. Reload the scene (TowerManager will now see Index 0 and reset the difficulty)
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}