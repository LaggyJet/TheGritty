//worked on by - natalie lubahn
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Audio;

public class SettingsManager : MonoBehaviour
{
    public TMP_Dropdown graphicsDropdown;
    public Slider masterVol;
    public Slider bgmVol;
    public Slider sfxVol;
    public AudioMixer audMix;

    public void ChangeGraphicsQuality()
    {
        QualitySettings.SetQualityLevel(graphicsDropdown.value); ;
    }
    public void ChangeMasterVolume()
    {
        audMix.SetFloat("masterVol", masterVol.value);
    }
    public void ChangeBgmVolume()
    {
        audMix.SetFloat("bgmVol", bgmVol.value);
    }
    public void ChangeSfxVolume()
    {
        audMix.SetFloat("sfxVol", sfxVol.value);
    }
}