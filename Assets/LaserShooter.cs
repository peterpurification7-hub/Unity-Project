using UnityEngine;
using StarterAssets;

public class LaserShooter : MonoBehaviour
{
    private FirstPersonController _controller;
    private LineRenderer _lineRenderer;

    [Header("Melee")]
    public float punchRange = 3.5f; // How close you need to be to punch

    [Header("Visuals")]
    public ParticleSystem laserParticles;
    public Material laserMaterial;
    [SerializeField] private float pulseDuration = 0.15f;

    [Header("Positioning")]
    // X = Right/Left, Y = Up/Down, Z = Forward/Back
    public Vector3 muzzleOffset = new Vector3(0.3f, -0.4f, 0.5f);

    void Start()
    {
        _controller = Object.FindFirstObjectByType<StarterAssets.FirstPersonController>();

        // Setup LineRenderer
        _lineRenderer = gameObject.AddComponent<LineRenderer>();

        // FIX: Use the material from the Inspector if you provided one!
        if (laserMaterial != null)
            _lineRenderer.material = laserMaterial;
        else
            _lineRenderer.material = new Material(Shader.Find("Legacy Shaders/Particles/Additive"));

        _lineRenderer.startColor = Color.cyan;
        _lineRenderer.endColor = Color.blue;
        _lineRenderer.startWidth = 0.5f;
        _lineRenderer.endWidth = 0.1f;
        _lineRenderer.enabled = false;
    }

    void Update()
    {
        if (PauseManager.isPaused) return;

        if (UnityEngine.InputSystem.Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (_controller != null)
            {
                if (_controller.canShoot)
                {
                    // RANGED MODE: Already has its own Raycast inside ShootLaser()
                    ShootLaser();
                }
                else
                {
                    // MELEE MODE: Let's check if we actually hit something!
                    PerformMeleeCheck();
                }
            }
        }
    }

    void ShootLaser()
    {
        if (AudioManager.instance != null)
            AudioManager.instance.PlaySFX(AudioManager.instance.shooting);
        // 1. Visual Effects
        if (laserParticles != null) laserParticles.Play();
        StopAllCoroutines();
        StartCoroutine(LaserPulseEffect());

        // 2. Setup the Math
        // We shoot from the center of the camera for perfect aim
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;
        int layerMask = ~LayerMask.GetMask("Player");

        // 3. Setup the Visuals
        // We start the beam at the offset (bottom-right) so it doesn't block the view
        Vector3 visualStartPoint = transform.TransformPoint(muzzleOffset);
        _lineRenderer.SetPosition(0, visualStartPoint);

        // 4. Execute the Hit
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask))
        {
            // The beam goes from our offset muzzle to the actual hit point
            _lineRenderer.SetPosition(1, hit.point);

            EnemyAI enemy = hit.collider.GetComponent<EnemyAI>();
            if (enemy != null)
            {
                enemy.TakeDamage(_controller.laserDamage);
            }

            Debug.Log("Hit: " + hit.collider.name);
        }
        else
        {
            // If we hit the sky, draw the line to a point 100m ahead of the CAMERA
            // This makes the laser appear to converge on your crosshair
            _lineRenderer.SetPosition(1, transform.position + (transform.forward * 100f));
        }
    }

    private System.Collections.IEnumerator LaserPulseEffect()
    {
        _lineRenderer.enabled = true;
        float elapsed = 0f;
        float originalStartWidth = 0.5f; // Match your Start() value

        while (elapsed < pulseDuration)
        {
            elapsed += Time.deltaTime;
            _lineRenderer.startWidth = Mathf.Lerp(originalStartWidth, 0f, elapsed / pulseDuration);
            yield return null;
        }

        _lineRenderer.enabled = false;
        _lineRenderer.startWidth = originalStartWidth;
    }

    void PerformMeleeCheck()
    {
        // 1. Give the punch a "thickness" (radius)
        float punchThickness = 0.6f;
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;
        int layerMask = ~LayerMask.GetMask("Player");

        // 2. Use SphereCast for "forgiving" hit detection
        if (Physics.SphereCast(ray, punchThickness, out hit, punchRange, layerMask))
        {
            EnemyAI enemy = hit.collider.GetComponent<EnemyAI>();

            if (enemy != null)
            {
                // SUCCESS: This logic now handles BOTH sound and damage
                if (AudioManager.instance != null)
                    AudioManager.instance.PlaySFX(AudioManager.instance.playerAttack);

                enemy.TakeDamage(1.0f);
                Debug.Log("LASER-SHOOTER: Hit " + hit.collider.name + " with a heavy punch!");
            }
        }
        else
        {
            Debug.Log("LASER-SHOOTER: Whiffed the punch.");
        }
    }
}