using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    [Header("UI Pages")]
    public GameObject page1;
    public GameObject page2;

    void Awake()
    {
        if (page1 != null) page1.SetActive(true);
        if (page2 != null) page2.SetActive(false);
    }

    void Update()
    {
        // THE NUCLEAR OPTION: Freeze time constantly while this menu is open.
        // No other script (like PauseCanvas) can override this now!
        Time.timeScale = 0f;

        // Aggressively force the mouse to stay visible
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void GoToNextPage()
    {
        if (page1 != null) page1.SetActive(false);
        if (page2 != null) page2.SetActive(true);
    }

    public void CloseTutorial()
    {
        Time.timeScale = 1f; // Finally allow time to unfreeze

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        gameObject.SetActive(false); // Turn off the tutorial
    }
}