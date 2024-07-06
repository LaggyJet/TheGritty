// Jacob, Kheera 

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class Class_Mage : MonoBehaviour
{
    PlayerController player;

    //these are combat variables
    //public bool isShooting;  //CAN BE DELETED BUT JUST IN CASE U NEED IT (moved to game manager)
    bool sprayingFire;

    [SerializeField] float primaryStamCost = 0.05f;  
    [SerializeField] float secondaryStamCost = 0.5f; 


    private void Start()
    {
        player = GameManager.instance.player.GetComponent<PlayerController>();
        player.combatObjects[1].SetActive(true);
        player.combatObjects[1].GetComponent<ParticleSystem>().Stop();
    }
    public void OnPrimaryFire(InputAction.CallbackContext ctxt)
    {

        if (ctxt.performed && !GameManager.instance.isShooting && !GameManager.instance.isPaused && SceneManager.GetActiveScene().name != "title menu" && player.staminaCor == null)
        {

            player.animate.SetTrigger("Mage1");
            player.SubtractStamina(0.5f);
        }
    }
    public void OnSecondaryFire(InputAction.CallbackContext ctxt)
    {
        if (ctxt.performed && !GameManager.instance.isPaused && SceneManager.GetActiveScene().name != "title menu")
        {
            if (!GameManager.instance.isShooting)
            {
                player.SubtractStamina(0.5f);
                GameManager.instance.isShooting = true;
                player.SetAnimationBool("Mage2", true);
                if (sprayingFire)
                    Instantiate(player.combatObjects[2], player.combatObjects[1].transform.position, player.combatObjects[1].transform.rotation);
            }

        }
        else if (ctxt.canceled && GameManager.instance.isShooting)
        {
            GameManager.instance.isShooting = false;
            player.SetAnimationBool("Mage2", false);
            SecondaryFireStop();
        }
    }

    //CAN BE DELETED BUT JUST IN CASE U NEED IT
    //void Update()
   // {
        //if (!GameManager.instance.isPaused && SceneManager.GetActiveScene().name != "title menu")
        //{
        //    FireCheck();
        //    if (sprayingFire)
        //        Instantiate(player.combatObjects[2], player.combatObjects[1].transform.position, player.combatObjects[1].transform.rotation);
        //}
   // }

    void PrimaryFire()
    {
        if(player.stamina >= primaryStamCost)
        {
        // Subtracts stamina when fired (can change stam cost in fields)
        player.SubtractStamina(primaryStamCost);

        //sets shootings variable to true so we can only fire once at a time
        GameManager.instance.isShooting = true;

        player.PlaySound('A');

        //spawns our projectile
        Instantiate(player.combatObjects[0], player.shootPosition.transform.position, player.shootPosition.transform.rotation);
        GameManager.instance.isShooting = false;
        }
        else
        {
           Debug.Log("No Staminaaaaaa :(");
        }

    }
   

    //CAN BE DELETED BUT JUST IN CASE U NEED IT
    //void FireCheck()
    //{
    //    if(Input.GetButtonDown("Fire1"))
    //    {
    //        //plays our primary shooting animation
    //        player.animate.SetTrigger("Mage1");
    //    }
    //    if(Input.GetButtonDown("Fire2") && !GameManager.instance.isShooting)
    //    {
    //        GameManager.instance.isShooting = true;
    //        player.SetAnimationBool("Mage2", true);
    //    }
    //    if (Input.GetButtonUp("Fire2") && GameManager.instance.isShooting)
    //    {
    //        GameManager.instance.isShooting = false;
    //        player.SetAnimationBool("Mage2", false);
    //        SecondaryFireStop();
    //    }
    //}

    void SecondaryFire()
    {
        if(player.stamina >= secondaryStamCost)
        {
            // Subtracts stamina when fired (can change stam cost in fields)
            player.SubtractStamina(secondaryStamCost);

         player.PlaySound('A');
         sprayingFire = true;
         player.combatObjects[1].GetComponent<ParticleSystem>().Play();
        }
        else
        {
            Debug.Log("No staminaaaa :(");
        }
        
    }

    void SecondaryFireStop()
    {
        player.combatObjects[1].GetComponent<ParticleSystem>().Stop(true, ParticleSystemStopBehavior.StopEmitting); 
        sprayingFire = false;
    }
}
