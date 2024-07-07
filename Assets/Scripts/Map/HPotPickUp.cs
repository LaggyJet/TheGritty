//Made by Emily Underwood, Kheera King 
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HPotPickUp : MonoBehaviour
{

    public enum SelectPotion {stamina, health}  
    // Amount of hp/stamina the potion adds to the player
    [Range(0f, 10f)] public float Amount;  
    public SelectPotion type;
    [SerializeField] AudioSource Audio;  
    [SerializeField] AudioClip pickUp;
    [SerializeField] float pickUpVol;
   
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Sound for player picking up potion 
            Audio.clip = pickUp; 
            Audio.volume = pickUpVol;
            Audio.Play();
            
            // If stamina or health 
            switch(type)
            {
                case SelectPotion.health:
                GameManager.instance.playerScript.AddHP(Amount);
                break;

                case SelectPotion.stamina:
                GameManager.instance.playerScript.AddStamina(Amount);
                break;

                default:
                Debug.Log("Wine? :)");
                break;
            }
            
            // Clean up 
            Destroy(gameObject);
        }
    }


}
