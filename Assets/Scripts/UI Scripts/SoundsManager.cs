// Kheera 

using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using Photon.Realtime;
using UnityEditor;
using UnityEngine;

public class SoundsManager : MonoBehaviour
{
    // Access
    public static SoundsManager instance;
    // Start is called before the first frame update
    void Awake()
    {
        // Check if sounds manager object exist and instance this one
        if(instance == null)
        {
          instance = this;
        }
    }

    void Update()
    {
       PlayNoHP();
       ShakeSTAM();
    }

    public void PlayNoHP()
    {
        // If display is < 50% & no hp sound is playing & already passed threshold of 50% for hp value
        if (PlayerController.instance!= null && PlayerController.instance.HpDisplay <= 0.5f && !PlayerController.instance.isPlayingNoHP && PlayerController.instance.aboveThresholdHP)
        {
            // Check if stamina audio source is not playing already
            if (!PlayerController.instance.staminaAudioSource.isPlaying)
            {
                // Play the clip at correct volume 
                PlayerController.instance.staminaAudioSource.PlayOneShot(PlayerController.instance.noHP[Random.Range(0, PlayerController.instance.noHP.Length)], PlayerController.instance.noHPvol);

                // Set playing no hp to on and then back off to use threshold bool again
                PlayerController.instance.isPlayingNoHP = true;
                PlayerController.instance.aboveThresholdHP = false; // Has been above threshold 
                
                // Set is :playing no hp: check back to off after clip is done playing 
                StartCoroutine(ResetNoHP(PlayerController.instance.noHP[0].length));
                Debug.Log("No HP");

                // Shake once for critical condition 
                if(!Shake.instance.isShaking && Shake.instance.hasShakenHP)
                {
                   PlayerController.instance.hpShake.Shaking(); // Turn on shake
                   Shake.instance.isShaking = true;
                   Shake.instance.hasShakenHP = false; // Has been above threshold
                }
            }
        }
        // If hp display is 50% over
        else if(PlayerController.instance != null && PlayerController.instance.HpDisplay > 0.5f)
        {
            // Setting the abovethreshold check back on 
           PlayerController.instance.aboveThresholdHP = true;
           Shake.instance.hasShakenHP = true;
        }
    }

    public void ShakeSTAM()
    {
            // Shake once for critical condition 
                if(PlayerController.instance != null && PlayerController.instance.abovethresholdSTAM && Shake.instance.hasShakenSTAM && PlayerController.instance.StaminaDisplay <= 0.5f)
                {
                   PlayerController.instance.stamShake.Shaking(); 
                   Shake.instance.isShaking = true;
                   Shake.instance.hasShakenSTAM = false; // Has been above threshold
                   Shake.instance.isShaking = false; // Turn shake back off
                }
                // If stamina is above 50%
                else if(PlayerController.instance != null && PlayerController.instance.StaminaDisplay > 0.5f)
                { 
                  // Set values on so shake can work if off
                  PlayerController.instance.abovethresholdSTAM = true;
                  Shake.instance.hasShakenSTAM = true;
                }
    }

    private IEnumerator ResetNoHP(float num)
    {
       yield return new WaitForSeconds(1);
       PlayerController.instance.isPlayingNoHP = false;
       Shake.instance.isShaking = false;
    }

}
