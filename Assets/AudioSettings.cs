using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class AudioSettings : MonoBehaviour
{
    public AudioMixer mainMixer;

    // Call these from the OnValueChanged event of your Sliders
    public void SetMasterVolume(float value)
    {
        // Mixers use a logarithmic scale (-80 to 20), so we use Log10
        mainMixer.SetFloat("MasterVol", Mathf.Log10(value) * 20);
    }

    public void SetSFXVolume(float value)
    {
        mainMixer.SetFloat("SFXVol", Mathf.Log10(value) * 20);
    }
}