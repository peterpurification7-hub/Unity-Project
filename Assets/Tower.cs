using UnityEngine;
using TMPro;
using UnityEngine.InputSystem; // <-- THE FIX: Tells Unity to use the modern Input System!

public class Tower : MonoBehaviour
{
    [Header("Requirements")]
    public int slimesNeeded = 10;
    // --- NEW: Custom text for the UI pop-up ---
    public string towerPerkMessage = "Gliding Unlocked!";
    public bool showActivationMessage = true;
    public bool isObjective = false;
    public bool isCompleted = false;

    [Header("Interaction Settings")]
    public float interactRange = 6f;

    [Header("Floating UI")]
    public GameObject floatingCanvas;
    public TextMeshProUGUI promptText;

    [Header("Visuals")]
    public ParticleSystem beamParticles;
    public Light towerLight;

    private PlayerStats playerStats;
    private Camera mainCam; // Added to track the player's eyes

    void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) playerStats = player.GetComponent<PlayerStats>();

        mainCam = Camera.main; // Cache the camera

        if (promptText != null)
            promptText.text = slimesNeeded + " Slimes Needed";

        if (floatingCanvas != null) floatingCanvas.SetActive(false);

        UpdateVisuals();
    }

    void Update()
    {
        if (isCompleted || !isObjective || playerStats == null)
        {
            if (floatingCanvas != null) floatingCanvas.SetActive(false);
            return;
        }

        float distance = Vector3.Distance(transform.position, playerStats.transform.position);

        if (distance <= interactRange)
        {
            if (floatingCanvas != null)
            {
                floatingCanvas.SetActive(true);

                // --- THE TRACKING FIX ---
                // Forces the UI to perfectly swivel and face the camera every frame
                if (mainCam != null)
                {
                    floatingCanvas.transform.LookAt(floatingCanvas.transform.position + mainCam.transform.rotation * Vector3.forward,
                             mainCam.transform.rotation * Vector3.up);
                }
            }

            // --- THE INPUT FIX ---
            // Safely asks the New Input System if 'E' was pressed this exact frame
            if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
            {
                Interact();
            }
        }
        else
        {
            if (floatingCanvas != null) floatingCanvas.SetActive(false);
        }
    }

    public void SetObjective(bool state)
    {
        isObjective = state;
        UpdateVisuals();
    }

    public void Interact()
    {
        if (isCompleted) return;

        if (playerStats.slimesSaved >= slimesNeeded)
        {
            CompleteTower();
        }
        else
        {
            int missing = slimesNeeded - playerStats.slimesSaved;
            ShowMessage("You need " + missing + " more slimes to restore this tower!");
        }
    }

    void CompleteTower()
    {
        playerStats.AddSlime(-slimesNeeded);
        isCompleted = true;
        isObjective = false;
        UpdateVisuals();

        TowerManager manager = Object.FindFirstObjectByType<TowerManager>();
        if (manager != null) manager.TowerActivated();

        // --- NEW: Only show the pop-up if the box is checked! ---
        if (showActivationMessage)
        {
            ShowMessage("Tower Activated!\n" + towerPerkMessage);
        }

        if (floatingCanvas != null) floatingCanvas.SetActive(false);
    }

    void ShowMessage(string text)
    {
        if (playerStats != null && playerStats.popUpText != null)
        {
            playerStats.popUpText.text = text;
            playerStats.popUpText.transform.parent.gameObject.SetActive(true);
            CancelInvoke("HideMessage");
            Invoke("HideMessage", 3f);
        }
    }

    void HideMessage() { if (playerStats != null) playerStats.HidePopUp(); }

    public void UpdateVisuals()
    {
        if (beamParticles == null || towerLight == null) return;

        Color targetColor = isCompleted ? Color.green : (isObjective ? Color.red : Color.clear);
        bool shouldBeVisible = isCompleted || isObjective;

        towerLight.enabled = shouldBeVisible;
        towerLight.color = targetColor;
        towerLight.intensity = 10f;

        var main = beamParticles.main;
        main.startColor = targetColor;

        if (shouldBeVisible)
        {
            if (!beamParticles.isPlaying) beamParticles.Play();

            Renderer r = beamParticles.GetComponent<Renderer>();
            if (r != null && r.material != null)
            {
                r.material.SetColor("_EmissionColor", targetColor * 2f);
                r.material.color = targetColor;
            }
        }
        else
        {
            beamParticles.Stop();
            beamParticles.Clear();
        }
    }

    public void SetCompleted()
    {
        isCompleted = true;
        isObjective = false;
        UpdateVisuals();
    }
}