using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Audio;

public class MusicManager : MonoBehaviour
{
    public static MusicManager instance;

    [Header("Mixer Setup")]
    public AudioMixerGroup musicGroup; // <-- 2. ADD THIS FIELD

    [Header("Exploration Music")]
    public AudioClip[] backgroundTracks;
    public float minPauseTime = 5f;
    public float maxPauseTime = 15f;

    [Header("Combat Music")]
    public AudioClip combatTrack;

    private AudioSource _explorationSource;
    private AudioSource _combatSource;
    private bool _inCombat = false;
    private int _alertedEnemies = 0;

    void Awake()
    {
        instance = this;
        // We use two sources so we can crossfade between them
        _explorationSource = gameObject.AddComponent<AudioSource>();
        _combatSource = gameObject.AddComponent<AudioSource>();

        // 3. ADD THESE TWO LINES TO HOOK UP THE MIXER
        _explorationSource.outputAudioMixerGroup = musicGroup;
        _combatSource.outputAudioMixerGroup = musicGroup;

        _explorationSource.loop = false; // We handle the "looping" via script
        _combatSource.loop = true;
        _combatSource.clip = combatTrack;
        _combatSource.volume = 0; // Start silent
    }

    void Start()
    {
        StartCoroutine(BackgroundPlaylist());
    }

    IEnumerator BackgroundPlaylist()
    {
        int currentTrack = 0;
        while (true)
        {
            if (!_inCombat)
            {
                _explorationSource.clip = backgroundTracks[currentTrack];
                _explorationSource.Play();

                // Wait for the track to finish
                yield return new WaitWhile(() => _explorationSource.isPlaying || _inCombat);

                // If we aren't in combat, wait for a natural pause
                if (!_inCombat)
                {
                    float pause = Random.Range(minPauseTime, maxPauseTime);
                    yield return new WaitForSeconds(pause);
                }

                currentTrack = (currentTrack + 1) % backgroundTracks.Length;
            }
            yield return null;
        }
    }

    public void UpdateCombatState(bool isAlerted)
    {
        if (isAlerted) _alertedEnemies++;
        else _alertedEnemies = Mathf.Max(0, _alertedEnemies - 1);

        bool shouldBeInCombat = _alertedEnemies > 0;

        if (shouldBeInCombat != _inCombat)
        {
            _inCombat = shouldBeInCombat;
            StopAllCoroutines();
            StartCoroutine(CrossfadeMusic());
            if (!_inCombat) StartCoroutine(BackgroundPlaylist()); // Resume playlist
        }
    }

    IEnumerator CrossfadeMusic()
    {
        float duration = 1.5f; // Seconds to fade
        float time = 0;

        float startExplorationVol = _explorationSource.volume;
        float startCombatVol = _combatSource.volume;

        if (_inCombat && !_combatSource.isPlaying) _combatSource.Play();

        while (time < duration)
        {
            time += Time.deltaTime;
            float percent = time / duration;

            if (_inCombat)
            {
                _explorationSource.volume = Mathf.Lerp(startExplorationVol, 0, percent);
                _combatSource.volume = Mathf.Lerp(startCombatVol, 0.5f, percent);
            }
            else
            {
                _explorationSource.volume = Mathf.Lerp(startExplorationVol, 0.5f, percent);
                _combatSource.volume = Mathf.Lerp(startCombatVol, 0, percent);
            }
            yield return null;
        }
    }
}