using UnityEngine;

public class WindFluctuation : MonoBehaviour
{
    private AudioSource _windSource;
    public float minVolume = 0.2f;
    public float maxVolume = 0.5f;

    void Start() => _windSource = GetComponent<AudioSource>();

    void Update()
    {
        // Use Perlin Noise to create a smooth, natural-sounding volume change
        float noise = Mathf.PerlinNoise(Time.time * 0.5f, 0f);
        _windSource.volume = Mathf.Lerp(minVolume, maxVolume, noise);
    }
}