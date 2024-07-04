//Made by Emily Underwood
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HPotPickUp : MonoBehaviour
{
    [SerializeField] float hpAdd; //amount of hp/stamina the potion adds to the player
    [SerializeField] float staminaAdd; 
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            GameManager.instance.playerScript.AddHP(hpAdd);
            Destroy(gameObject);
        }
    }
// Need to adjust script to accept stamina potions 

}
