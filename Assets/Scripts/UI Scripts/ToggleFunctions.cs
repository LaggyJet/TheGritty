using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToggleFunctions : MonoBehaviour
{
     public Toggle toggle;

     public void Toggle(bool var)
    {
        var = !var;
    }
    public void isOnToggle()
    {
        toggle.isOn = false;
    }    
}
