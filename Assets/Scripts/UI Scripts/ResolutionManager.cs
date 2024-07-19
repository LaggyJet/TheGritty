//worked on by - natalie lubahn, Kheera
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Diagnostics.CodeAnalysis;
using UnityEngine.Audio;

public class ResolutionManager : MonoBehaviour
{
    //variables
    [SerializeField] TMP_Dropdown resDropDown;
    private static int resDDVal;

    private Resolution[] resolutions;
    private List<Resolution> filteredResolutions;
    private float currentRefreshRate;
    private int currentResolutionIndex;
    private Resolution resolution;
    public static ResolutionManager instance { get; private set; }

    private void Awake()
    {
        instance = this;
        if (instance != null)
        {
            LoadPrefs();
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        //initializes array and list
        resolutions = Screen.resolutions;
        filteredResolutions = new List<Resolution>();

        //sets up drop down and gets refresh rate for list
        if(resDropDown != null)
        {
            resDropDown.ClearOptions();
            currentRefreshRate = (float)Screen.currentResolution.refreshRateRatio.value;

            //fills filtered list
            for (int i = 0; i < resolutions.Length; i++)
            {
                if ((float)resolutions[i].refreshRateRatio.value == currentRefreshRate)
                {
                    filteredResolutions.Add(resolutions[i]);
                }
            }

            //creates the string and adds to the dropdown
            List<string> options = new List<string>();
            for (int i = 0; i < filteredResolutions.Count; i++)
            {
                string resolutionOption = filteredResolutions[i].width + "x" + filteredResolutions[i].height + " - " + (int)filteredResolutions[i].refreshRateRatio.value + " Hz";
                options.Add(resolutionOption);
                if (filteredResolutions[i].width == Screen.width && filteredResolutions[i].height == Screen.height)
                {
                    currentResolutionIndex = i;
                }
            }

            resDropDown.AddOptions(options);
            resDropDown.value = currentResolutionIndex;
            resDropDown.RefreshShownValue();

            if(resDDVal != currentResolutionIndex)
            {
                SetResolution(resDDVal);
            }
        }

    }

    public void SetResolution(int resolutionIndex)
    {
        GameManager.instance.PlayButtonClick();
        resolution = filteredResolutions[resolutionIndex];
        Screen.SetResolution(resolution.width, resolution.height, true);  
        resDDVal = resDropDown.value;
    }
    public void SavePrefs()
    {
        PlayerPrefs.SetInt("resDrpDwnVal", resDDVal);
        PlayerPrefs.Save();
    }

    public void LoadPrefs()
    {
        resDDVal = PlayerPrefs.GetInt("resDrpDwnVal", resDDVal);
    }
}
