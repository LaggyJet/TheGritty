//Worked on By : Jacob Irvin, Joshua Furber
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;

public class Class_Mage : MonoBehaviour
{
    PlayerController player;

    //these are combat variables
    bool isShooting;
    bool sprayingFire;


    private void Start()
    {
        player = GameManager.instance.player.GetComponent<PlayerController>();
        player.combatObjects[1].SetActive(true);
        player.combatObjects[1].GetComponent<ParticleSystem>().Stop();
    }
    
    void Update()
    {
        if (!GameManager.instance.isPaused && SceneManager.GetActiveScene().name != "title menu")
        {
            FireCheck();
            if (sprayingFire) {
                if (PhotonNetwork.IsConnected)
                    PhotonNetwork.Instantiate(player.combatObjects[2].name, player.combatObjects[1].transform.position, player.combatObjects[1].transform.rotation);
                else if (!PhotonNetwork.IsConnected)
                    Instantiate(player.combatObjects[2], player.combatObjects[1].transform.position, player.combatObjects[1].transform.rotation)
            }
        }

    }

    void PrimaryFire()
    {
        //sets shootings variable to true so we can only fire once at a time
        isShooting = true;


        player.PlaySound('A');

        //spawns our projectile
        if (PhotonNetwork.IsConnected)
            PhotonNetwork.Instantiate(player.combatObjects[0].name, player.shootPosition.transform.position, player.shootPosition.transform.rotation);
        else if (!PhotonNetwork.IsConnected)
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
        player.combatObjects[1].GetComponent<ParticleSystem>().Play();
    }

    void SecondaryFireStop()
    {
        player.combatObjects[1].GetComponent<ParticleSystem>().Stop(true, ParticleSystemStopBehavior.StopEmitting); 
        sprayingFire = false;
    }
}
