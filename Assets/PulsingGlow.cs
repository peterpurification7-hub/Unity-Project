using UnityEngine;

public class PulsingGlow : MonoBehaviour
{
    public Color glowColor = Color.red;
    public float pulseSpeed = 2.0f;
    public float minIntensity = 0.5f;
    public float maxIntensity = 5.0f;

    private Renderer rend;
    private Material mat;

    void Start()
    {
        rend = GetComponent<Renderer>();
        // We use .material (not .sharedMaterial) so each enemy pulses at its own time
        mat = rend.material;
        mat.EnableKeyword("_EMISSION");
    }

    void Update()
    {
        // Mathf.Sin goes from -1 to 1. 
        // We use (sin + 1) / 2 to make it go from 0 to 1.
        float lerp = (Mathf.Sin(Time.time * pulseSpeed) + 1.0f) / 2.0f;

        // Calculate the current brightness
        float intensity = Mathf.Lerp(minIntensity, maxIntensity, lerp);

        // Apply the color and intensity to the material
        // "_EmissionColor" is the internal name Unity uses for the glow
        Color finalColor = glowColor * intensity;
        mat.SetColor("_EmissionColor", finalColor);
    }
}