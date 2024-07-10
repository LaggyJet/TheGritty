//Worked on By : Jacob Irvin, Joshua Furber
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using Photon.Pun;

public class Class_Archer : MonoBehaviourPun
{
    PlayerController player;

    bool primaryCooldown;
    bool secondaryCooldown;
    bool isCounting;
    int abilityCoolDown;
    int abilityActive;

    float primaryStamCost = 0.3f;
    float secondaryStamCost = 0.3f;

    // Start is called before the first frame update
    void Start()
    {
        // Set the player set and save for access later
        player = GetComponent<PlayerController>();
    }

    void Update()
    {
        if (abilityActive > 0)
        {
            StartCoroutine(AbilityActive());
        }
        else if (abilityCoolDown > 0)
        {
            StartCoroutine(AbilityCountDown());
        }
    }

    public void OnPrimaryFire(InputAction.CallbackContext ctxt)
    {

        //checks for if our input was performed and if its valid to attack
        //check the ValidAttack Function to see what quantifies as valid
        //also checks if we have the stamina to attack
        if (ctxt.performed && ValidAttack() && StaminaCheck(primaryStamCost))
        {
            GameManager.instance.isShooting = true;
            if (player.useStamina)
                player.stamina -= primaryStamCost;
            //starts our mage primary attack animation and plays our associated sound
            player.SetAnimationTrigger("Archer1");
            player.PlaySound('A');


            //!Remove once arrows spawn through animation stuff
            ShootArrow(1);
        }
        //checks for if the input was performed in a valid context but there isn't enough stamina to perform our action
        else if (ctxt.performed && ValidAttack() && !StaminaCheck(primaryStamCost))
        {
            // Checking for audio ( preventing looping on sounds )
            if (!player.staminaAudioSource.isPlaying)
            {
                // Play out of stamina sound 
                player.staminaAudioSource.PlayOneShot(player.noAttack[Random.Range(0, player.noAttack.Length)], player.noAttackVol);
                player.isPlayingStamina = true;
            }
            player.isPlayingStamina = player.staminaAudioSource.isPlaying;
        }
    }
    public void OnSecondaryFire(InputAction.CallbackContext ctxt)
    {
        //Checks the same things as primary attack
        if (ctxt.performed && !GameManager.instance.isPaused && SceneManager.GetActiveScene().name != "title menu")
        {
            GameManager.instance.isShooting = true;
            if (player.useStamina)
                player.stamina -= secondaryStamCost;
            //starts our mage primary attack animation and plays our associated sound
            player.SetAnimationTrigger("Archer2");
            player.PlaySound('A');

            //!Remove once arrows spawn through animation stuff
            ShootArrow(3);
        }
        //if input is pressed and the context is valid but we don't have enough stamina run this code
        else if (ctxt.performed && ValidAttack() && !StaminaCheck(secondaryStamCost))
        {
            // Checking for audio ( preventing looping on sounds )
            if (!player.staminaAudioSource.isPlaying)
            {
                // Play out of stamina sound
                player.staminaAudioSource.PlayOneShot(player.noAttack[Random.Range(0, player.noAttack.Length)], player.noAttackVol);
                player.isPlayingStamina = true;
            }
            player.isPlayingStamina = player.staminaAudioSource.isPlaying;
        }
        //if we stop holding the input this code runs
        else if (ctxt.canceled && GameManager.instance.isShooting)
        {
            //sets us to not attacking, sets our animation bool to false so we can end the animation, and stops our particle system and coroutine
            GameManager.instance.isShooting = false;
            player.SetAnimationBool("Archer2", false);
        }
    }

    public void OnAbility(InputAction.CallbackContext ctxt)
    {
        //checks that we are not on cooldown and not using the ability
        if (ctxt.performed && ValidAttack() && abilityCoolDown == 0 && abilityActive == 0)
        {
            //plays our animation and sound for the ability
            player.SetAnimationTrigger("Archer3");
            player.PlaySound('A');
            //sets our active time to 3 and our cooldown time to 10
            abilityActive = 3;
            abilityCoolDown = 10;
            //makes stamina maximum so we pass all stamina checks during our ability time
            player.stamina = 10;
        }
    }

    //function for counting down our active time
    IEnumerator AbilityActive()
    {
        if (!isCounting)
        {
            if (player.useStamina)
            {
                player.useStamina = false;
            }
            isCounting = true;
            yield return new WaitForSeconds(1);
            abilityActive--;
            isCounting = false;
        }
    }
    //function for counting down our cooldown
    IEnumerator AbilityCountDown()
    {
        if (!isCounting)
        {
            if (!player.useStamina)
            {
                player.useStamina = true;
            }
            isCounting = true;
            yield return new WaitForSeconds(1);
            abilityCoolDown--;
            isCounting = false;
        }
    }

    // Function to spawn in desired amount of arrows
    void ShootArrow(int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            // Spawn multiplayer arrows if in multiplayer
            if (PhotonNetwork.InRoom && photonView.IsMine)
            {
                PhotonNetwork.Instantiate("Player/" + player.combatObjects[5].name, player.shootPosition.transform.position, player.shootPosition.transform.rotation);
            }
            // Otherwise spawn regularly 
            else if (!PhotonNetwork.IsConnected)
            {
                Instantiate(player.combatObjects[5], player.shootPosition.transform.position, player.shootPosition.transform.rotation);
            }
        }
    }

    //a function for checking certain conditions to see if it is appropriate to perform an action
    bool ValidAttack()
    {
        //checks these things:
        // 1. are we currently attacking
        // 2. are we paused
        // 3. are we in a valid scene
        if (!GameManager.instance.isShooting && !GameManager.instance.isPaused && SceneManager.GetActiveScene().name != "title menu")
        {
            return true;
        }
        return false;
    }

    //checks if we have the required stamina to perform the action
    bool StaminaCheck(float staminaRequired)
    {
        if (player.stamina >= staminaRequired)
        {
            return true;
        }
        return false;
    }
}
