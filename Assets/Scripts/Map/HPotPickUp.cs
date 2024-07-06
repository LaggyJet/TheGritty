//Made by Emily Underwood, Kheera King 
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HPotPickUp : MonoBehaviour
{
    // Amount of hp/stamina the potion adds to the player
    [Range(0f, 10f)] public float hpAdd;  
    [Range(0f, 10f)] public float staminaAdd; 
    [SerializeField] public AudioSource Audio; 
    [SerializeField] AudioClip pickUp;
   
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Audio.clip = pickUp; 
            Audio.Play();
            GameManager.instance.playerScript.AddHP(hpAdd);
            GameManager.instance.playerScript.AddStamina(staminaAdd);
            Destroy(gameObject);
        }
    }


}
