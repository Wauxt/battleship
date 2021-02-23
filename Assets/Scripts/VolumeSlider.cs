using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class VolumeSlider : MonoBehaviour
{
    public AudioMixer mixer;
    public void SetMasterVolume(float sliderValue) => mixer.SetFloat("Master", Mathf.Log10(sliderValue) * 20);
    public void SetUIVolume(float sliderValue) => mixer.SetFloat("UI", Mathf.Log10(sliderValue) * 20);
    public void SetShotsVolume(float sliderValue) => mixer.SetFloat("Shots", Mathf.Log10(sliderValue) * 20);
    public void SetSoundtrackVolume(float sliderValue) => mixer.SetFloat("Soundtrack", Mathf.Log10(sliderValue) * 20);

}
