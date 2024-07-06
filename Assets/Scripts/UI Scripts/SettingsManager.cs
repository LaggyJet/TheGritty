//worked on by - natalie lubahn
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Audio;
using System.Security.Cryptography.X509Certificates;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager instance { get; private set; }

    public TMP_Dropdown graphicsDropdown;
    public static int graphicsDDVal;
    public Slider masterVol;
    public static float masterVal;
    public Slider bgmVol;
    public static float bgmVal;
    public Slider sfxVol;
    public static float sfxVal;
    public AudioMixer audMix;

    private void Awake()
    {
        instance = this;
        if (instance != null)
        {
            LoadPrefs();
        }
    }
    private void Start()
    {
        graphicsDropdown.value = graphicsDDVal;
        masterVol.value = masterVal;
        bgmVol.value = bgmVal;
        sfxVol.value = sfxVal;
    }
    public void ChangeGraphicsQuality()
    {
        QualitySettings.SetQualityLevel(graphicsDropdown.value); ;
        graphicsDDVal = graphicsDropdown.value;
    }
    public void ChangeMasterVolume()
    {
        audMix.SetFloat("masterVol", masterVol.value);
        masterVal = masterVol.value;
    }
    public void ChangeBgmVolume()
    {
        audMix.SetFloat("bgmVol", bgmVol.value);
        bgmVal = bgmVol.value;
    }
    public void ChangeSfxVolume()
    {
        audMix.SetFloat("sfxVol", sfxVol.value);
        sfxVal = sfxVol.value;
    }

    public void defaultAudio()
    {
        masterVol.value = 0;
        bgmVol.value = 0;
        sfxVol.value = 0;
        masterVal = masterVol.value;
        bgmVal = bgmVol.value;
        sfxVal = sfxVol.value;
    }

    public void SavePrefs()
    {
        PlayerPrefs.SetInt("graphicsDrpDwnVal", graphicsDDVal);
        PlayerPrefs.SetFloat("MasterVol", masterVal);
        PlayerPrefs.SetFloat("BgmVol", bgmVal);
        PlayerPrefs.SetFloat("SfxVol", sfxVal);
        PlayerPrefs.Save();
    }

    public void LoadPrefs()
    {
        graphicsDDVal = PlayerPrefs.GetInt("graphicsDrpDwnVal", graphicsDDVal);
        masterVal = PlayerPrefs.GetFloat("MasterVol", masterVal);
        bgmVal = PlayerPrefs.GetFloat("BgmVol", bgmVal);
        sfxVal = PlayerPrefs.GetFloat("SfxVol", sfxVal);
    }
}