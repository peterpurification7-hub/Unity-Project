using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using StarterAssets;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    public static bool isPaused = false;
    public GameObject pauseMenuUI; // This should be your "PausePanel"
    public Slider sensitivitySlider;
    public GameObject hudCanvas; // Drag your HUDCanvas here

    private FirstPersonController playerController;

    void Awake()
    {
        Debug.Log("PauseManager: I am awake and running on " + gameObject.name);
    }

    void Start()
    {
        playerController = FindFirstObjectByType<FirstPersonController>();

        // Ensure the menu is hidden immediately
        if (pauseMenuUI != null)
        {
            pauseMenuUI.SetActive(false);
        }

        // CRITICAL: Force the game to "Resume" state on start 
        // so the camera knows it's allowed to move.
        isPaused = false;
        Time.timeScale = 1f;

        // Call Resume one time to sync the mouse settings
        Resume();

        if (playerController != null && sensitivitySlider != null)
            sensitivitySlider.value = playerController.RotationSpeed;
    }

    void Update()
    {
        // 1. Check if Keyboard exists
        if (Keyboard.current == null) return;

        // 2. Check for the P key
        if (Keyboard.current.pKey.wasPressedThisFrame)
        {
            Debug.Log("PauseManager: 'P' Key detected!");
            TogglePause();
        }
    }

    public void TogglePause()
    {
        if (isPaused) Resume();
        else Pause();
    }

    public void Resume()
    {
        Debug.Log("PauseManager: Resuming Game");
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        SetPlayerInput(true);

        hudCanvas.SetActive(true); // Show crosshair when playing
    }

    void Pause()
    {
        Debug.Log("PauseManager: Pausing Game");
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f;
        isPaused = true;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        SetPlayerInput(false);

        hudCanvas.SetActive(false); // Hide crosshair while paused
    }

    // Helper to stop the player from looking around while paused
    void SetPlayerInput(bool state)
    {
        var inputs = FindFirstObjectByType<StarterAssetsInputs>();
        if (inputs != null)
        {
            inputs.cursorLocked = state;
            inputs.cursorInputForLook = state;

            // NEW: Force the cursor state directly through the Starter Assets' own logic
            // This ensures the internal 'look' vector isn't trapped at zero.
            if (state)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
            else
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }
    }

    public void SetSensitivity(float value)
    {
        // 1. Double check the reference
        if (playerController == null)
        {
            playerController = Object.FindFirstObjectByType<FirstPersonController>();
        }

        if (playerController != null)
        {
            // 2. Update the RotationSpeed (or mouseSensitivity if you added that)
            playerController.RotationSpeed = value;

            // 3. Log it so you can see it working in the console
            Debug.Log("Sensitivity Updated to: " + value);
        }
    }

    public void PlayAgain()
    {
        // Ensure time is moving again in case the win screen paused it
        Time.timeScale = 1f;

        // Reload the current active scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void GoToMainMenu()
    {
        // 1. CRITICAL: Reset time to normal speed.
        // If you don't do this, the next time you start the game, 
        // it will be stuck at 0 speed (frozen).
        Time.timeScale = 1f;

        // 2. Load the Main Menu scene.
        // Replace "MainMenu" with the exact name of your menu scene 
        // as it appears in your Build Settings.
        SceneManager.LoadScene(0);
    }
}