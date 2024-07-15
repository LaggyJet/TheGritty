// Kheera 

using System.Collections; 
using System.Collections.Generic;
using UnityEngine;

public class Shake : MonoBehaviour
{
    // Determines the Position, Rotation, and Scale of said GameObject in the scene
    Transform targetObject;
    Vector3 initialPosition;

    // Public access
    public static Shake instance;

    // Controlling shaking & duration 
    [Range(0f, 10f)] public float shakeDuration;
    public bool hasShakenHP;
    public bool hasShakenSTAM;
    public bool isShaking = false; 

    // Implementing my 1-10 system 
    [Range(0f, 10f)]public float intensity; 



    // Start is called before the first frame update
    void Start()
    {

        if(instance == null)
        {
           instance = this;
        }

        // Storing position, rotate, scale in target
        targetObject = GetComponent<Transform>(); 

        // Get Local position of object 
        initialPosition = targetObject.localPosition;

        // Need this check to control shaking more than once
            hasShakenHP = PlayerController.instance.HpDisplay > 0.5f;
            hasShakenSTAM = PlayerController.instance.StaminaDisplay > 0.5f;
    }

    
    public void Shaking()
    {
        // Duration should be set in unity
        if (shakeDuration > 0)
        {

          // Check to make sure shaking is not happening first 
          if (isShaking)
          {
            // Reset to start
            StopCoroutine(DoShake());
            isShaking = false;
            targetObject.localPosition = initialPosition;
          }

           // Begin shaking frames 
           StartCoroutine(DoShake());
        }
       
    }

    IEnumerator DoShake()
    {

        GameManager.instance.hasRespawned = false;
        // Start shaking 
        isShaking = true;
        float num = 0f;

        while (num < shakeDuration)
        {
            if (GameManager.instance != null && !GameManager.instance.isPaused)   
            { 
            // Generate random position, keep local position ( DO NOT ADJUST OBJECT LOCATION COMPLETELY !!!)
            var random = new Vector3(Random.Range(-1f, 1f) * intensity, Random.Range(-1f, 1f) * intensity, 0f);
            // Set random postion for object 
            targetObject.localPosition = initialPosition + random;
            
            }
            // Wait for Next frame 
            yield return null;
            num += Time.deltaTime;
        }

        // Stop shaking - routine is finished 
        // Reset 
        targetObject.localPosition = initialPosition;
        isShaking = false;
       
    }

}
