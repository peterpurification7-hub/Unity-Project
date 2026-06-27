using UnityEngine;
using System.Collections;

public class EnemyAI : MonoBehaviour
{
    [Header("Stats (Updated by Difficulty)")]
    public float enemyHealth = 2f;    // This is the "Live" health
    public float damageAmount = 15f;  // This is the "Live" damage

    private float initialMaxHealth;

    [Header("Health UI")]
    public GameObject healthBarObject; // The Canvas we made
    public UnityEngine.UI.Slider healthSlider; // The Slider inside it

    [Header("Movement")]
    public float moveSpeed = 3.5f;
    public float detectionRange = 25f;
    public float exitDetectionRange = 32f;
    public float stoppingDistance = 2.5f;
    public float hoverHeight = 2.0f;
    public float heightSmoothSpeed = 5.0f;

    [Header("Combat")]
    public float attackCooldown = 2.0f;

    [Header("Loot")]
    public GameObject slimePrefab;

    private Transform player;
    private Terrain worldTerrain;
    private bool isAttacking = false;
    private float lastAttackTime;


    private bool _isAlerted = false;

    void Awake()
    {
        ApplyDifficultySettings();
    }

    void Start()
    {

        // 1. Better Player Search: Fallback to finding by tag if null
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;

        // 2. Better Terrain Search
        worldTerrain = Terrain.activeTerrain;

        if (worldTerrain == null) Debug.LogWarning(gameObject.name + ": No Terrain found in scene! Hovering may be disabled.");

        // Set the UI max value to whatever the difficulty decided
        initialMaxHealth = enemyHealth;

        if (healthBarObject != null)
            healthBarObject.SetActive(false);

    }

    void Update()
    {
        // Don't do anything if we are attacking or the player is missing
        if (isAttacking || player == null) return;

        float distance = Vector3.Distance(transform.position, player.position);

        // --- 1. STATE LOGIC (Entering and Exiting Combat) ---

        // ENTER: Player gets too close
        if (distance < detectionRange && !_isAlerted)
        {
            _isAlerted = true;
            if (MusicManager.instance != null) MusicManager.instance.UpdateCombatState(true);
            if (AudioManager.instance != null)
                AudioManager.instance.PlaySFX(AudioManager.instance.enemyAlert);
        }
        // EXIT: Player runs far enough away
        else if (distance > exitDetectionRange && _isAlerted)
        {
            _isAlerted = false;
            if (MusicManager.instance != null) MusicManager.instance.UpdateCombatState(false);
        }

        // --- 2. BEHAVIOR LOGIC (Only happens while Alerted) ---

        if (_isAlerted)
        {
            // ORIENTATION: Face the player
            transform.LookAt(player.position);

            // MOVEMENT: Chase the player until stoppingDistance
            if (distance > stoppingDistance)
            {
                Vector3 direction = (player.position - transform.position).normalized;
                Vector3 horizontalMove = new Vector3(direction.x, 0, direction.z) * moveSpeed * Time.deltaTime;
                transform.position += horizontalMove;
            }

            // HOVER: Keep floating above the terrain
            if (worldTerrain != null)
            {
                float groundHeight = worldTerrain.SampleHeight(transform.position);
                float targetY = groundHeight + worldTerrain.transform.position.y + hoverHeight;
                float newY = Mathf.Lerp(transform.position.y, targetY, Time.deltaTime * heightSmoothSpeed);
                transform.position = new Vector3(transform.position.x, newY, transform.position.z);
            }

            // ATTACK: If in range and cooldown is over, lunge!
            if (distance <= stoppingDistance && Time.time > lastAttackTime + attackCooldown)
            {
                StartCoroutine(AttackBounce());
            }
        }
    }

    IEnumerator AttackBounce()
    {
        // ADD THIS AT THE TOP
        if (AudioManager.instance != null)
            AudioManager.instance.PlaySFX(AudioManager.instance.enemyAttack);

        isAttacking = true;
        lastAttackTime = Time.time;
        Vector3 startPos = transform.position;

        // Lunge Forward
        float percent = 0;
        while (percent < 1)
        {
            percent += Time.deltaTime * 10f;
            Vector3 horizontalPos = Vector3.Lerp(startPos, player.position, percent);

            // Check terrain before setting height
            float yPos = horizontalPos.y;
            if (worldTerrain != null)
                yPos = worldTerrain.SampleHeight(horizontalPos) + worldTerrain.transform.position.y + hoverHeight;

            transform.position = new Vector3(horizontalPos.x, yPos, horizontalPos.z);
            yield return null;
        }

        // 3. Safe Damage Dealing
        if (player != null)
        {
            PlayerStats stats = player.GetComponent<PlayerStats>();
            if (stats != null)
            {
                stats.health -= damageAmount;
                stats.UpdateUI();
            }
        }

        // Bounce Back
        percent = 0;
        while (percent < 1)
        {
            percent += Time.deltaTime * 10f;
            Vector3 horizontalPos = Vector3.Lerp(player.position, startPos, percent);

            float yPos = horizontalPos.y;
            if (worldTerrain != null)
                yPos = worldTerrain.SampleHeight(horizontalPos) + worldTerrain.transform.position.y + hoverHeight;

            transform.position = new Vector3(horizontalPos.x, yPos, horizontalPos.z);
            yield return null;
        }

        isAttacking = false;
    }

    // Update TakeDamage to use the initialMaxHealth for the UI
    public void TakeDamage(float amount)
    {
        float actualDamage = amount / TowerManager.globalDifficulty;
        enemyHealth -= actualDamage;

        if (healthBarObject != null)
        {
            if (!healthBarObject.activeSelf)
            {
                healthBarObject.SetActive(true);
                healthSlider.maxValue = initialMaxHealth;
            }
            healthSlider.value = enemyHealth;
        }

        if (enemyHealth <= 0) StartCoroutine(DieSequence());
    }


    IEnumerator DieSequence()
    {
        // 1. Play the Death sound immediately upon hit
        if (AudioManager.instance != null)
            AudioManager.instance.PlaySFX(AudioManager.instance.enemyDeath);

        MusicManager.instance.UpdateCombatState(false);

        // 2. WAIT for the punch/hit impact to finish (approx. 0.15 seconds)
        yield return new WaitForSeconds(0.15f);

        // 3. NOW play the slime drop sound and spawn loot
        if (AudioManager.instance != null)
            AudioManager.instance.PlaySFX(AudioManager.instance.slimeDrop);

        if (slimePrefab != null)
        {
            int dropCount = Random.Range(3, 6);
            float scatterRadius = 1.5f;

            for (int i = 0; i < dropCount; i++)
            {
                Vector2 randomCircle = Random.insideUnitCircle * scatterRadius;
                Vector3 spawnPos = transform.position + new Vector3(randomCircle.x, 0.5f, randomCircle.y);

                float finalY = transform.position.y + 0.5f;
                if (worldTerrain != null)
                {
                    finalY = worldTerrain.SampleHeight(spawnPos) + worldTerrain.transform.position.y + 0.5f;
                }

                GameObject loot = Instantiate(slimePrefab, new Vector3(spawnPos.x, finalY, spawnPos.z), Quaternion.identity);
                loot.SetActive(true);

                Rigidbody rb = loot.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.AddForce(Vector3.up * 2f, ForceMode.Impulse);
                }
            }
        }

        Destroy(gameObject);
    }

    void ApplyDifficultySettings()
    {
        Debug.Log("ENEMY SPAWNED: Reading difficulty as " + GameSettings.SelectedDifficulty);

        switch (GameSettings.SelectedDifficulty)
        {
            case GameSettings.Difficulty.Easy:
                enemyHealth = 1f;
                damageAmount = 5f;
                break;
            case GameSettings.Difficulty.Normal:
                enemyHealth = 2f;
                damageAmount = 15f;
                break;
            case GameSettings.Difficulty.Hard:
                enemyHealth = 10f;
                damageAmount = 40f;
                break;
        }
    }
}