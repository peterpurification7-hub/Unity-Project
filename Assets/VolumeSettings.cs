using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class VolumeSettings : MonoBehaviour
{
    [Header("Mixer Reference")]
    public AudioMixer mainMixer;

    [Header("Slider References")]
    public Slider masterSlider;
    public Slider musicSlider;
    public Slider sfxSlider;

    public void SetMasterVolume()
    {
        float value = masterSlider.value;
        // Use Log10 to make the volume change sound "natural" to human ears
        mainMixer.SetFloat("MasterVol", Mathf.Log10(value) * 20);
    }

    public void SetMusicVolume()
    {
        float value = musicSlider.value;
        mainMixer.SetFloat("MusicVol", Mathf.Log10(value) * 20);
    }

    public void SetSfxVolume()
    {
        float value = sfxSlider.value;
        mainMixer.SetFloat("SfxVol", Mathf.Log10(value) * 20);
    }
}