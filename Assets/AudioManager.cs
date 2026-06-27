using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    [Header("Audio Sources")]
    public AudioSource musicSource;
    public AudioSource sfxSource;
    public AudioSource glideSource;

    [Header("Nature & Environment")]
    public AudioClip natureBackground;
    public AudioClip towerActivation;

    [Header("Player Sounds")]
    public AudioClip walking;
    public AudioClip jumping;
    public AudioClip landing; // <--- ADD THIS LINE
    public AudioClip gliding;
    public AudioClip shooting;
    public AudioClip playerAttack;
    public AudioClip slimePickup;

    [Header("Enemy Sounds")]
    public AudioClip enemyAlert;
    public AudioClip enemyAttack;
    public AudioClip enemyDeath; // <--- ADD THIS
    public AudioClip slimeDrop;  // <--- ADD THIS

    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        // Start the nature background immediately
        musicSource.clip = natureBackground;
        musicSource.loop = true;
        musicSource.Play();
    }

    public void PlaySFX(AudioClip clip, float volume = 1f)
    {
        if (clip != null)
        {
            sfxSource.PlayOneShot(clip, volume);
        }
    }
}