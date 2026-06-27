using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    // Make sure the name in quotes matches your actual Game Scene name exactly!
    public string gameSceneName = "SampleScene";

    public void PlayGame()
    {
        // --- NEW: Hard Reset the checkpoint memory for a fresh game ---
        GameSettings.LastCheckpointIndex = 0;

        SceneManager.LoadScene(gameSceneName);
    }

    public void QuitGame()
    {
        Debug.Log("QUIT!"); // This only shows in the editor
        Application.Quit(); // This closes the actual .exe or .app
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}