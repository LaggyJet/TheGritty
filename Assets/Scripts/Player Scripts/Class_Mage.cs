using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Class_Mage : MonoBehaviour
{
    PlayerController player;

    //these are combat variables
    bool isShooting;
    bool sprayingFire;


    private void Start()
    {
        player = GameManager.instance.player.GetComponent<PlayerController>();
    }
    
    void Update()
    {
        player.combatObjects[1].SetActive(true);
        player.combatObjects[1].GetComponent<ParticleSystem>().Stop(true);
        if (!GameManager.instance.isPaused && SceneManager.GetActiveScene().name != "title menu")
        {
            FireCheck();
            if (sprayingFire)
                Instantiate(player.combatObjects[2], player.combatObjects[1].transform.position, player.combatObjects[1].transform.rotation);
        }
    }

    void PrimaryFire()
    {
        //sets shootings variable to true so we can only fire once at a time
        isShooting = true;


        player.PlaySound('A');

        //spawns our projectile
        Instantiate(player.combatObjects[0], player.shootPosition.transform.position, player.shootPosition.transform.rotation);
        isShooting = false;
    }

    void FireCheck()
    {
        if(Input.GetButtonDown("Fire1"))
        {
            //plays our primary shooting animation
            player.animate.SetTrigger("Mage1");
        }
        if(Input.GetButtonDown("Fire2") && !isShooting)
        {
            isShooting = true;
            player.SetAnimationBool("Mage2", true);
        }
        if (Input.GetButtonUp("Fire2") && isShooting)
        {
            isShooting = false;
            player.SetAnimationBool("Mage2", false);
            SecondaryFireStop();
        }
    }

    void SecondaryFire()
    {
        player.PlaySound('A');
        sprayingFire = true;
        Debug.Log("Brother What");
        player.combatObjects[1].GetComponent<ParticleSystem>().Play();
        player.combatObjects[1].GetComponent<ParticleSystem>().Stop(false);
    }

    void SecondaryFireStop()
    {
        player.combatObjects[1].GetComponent<ParticleSystem>().Stop(true, ParticleSystemStopBehavior.StopEmitting); 
        sprayingFire = false;
    }
}
