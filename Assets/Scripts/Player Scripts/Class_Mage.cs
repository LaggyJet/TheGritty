//Worked on By : Jacob Irvin, Joshua Furber, Kheera

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using Photon.Pun;


/// <hello everyone>
/// PLEASE FOR THE LOVE OF EVERYTHING IF U HAVE TO TOUCH MY SCRIPTS AT LEAST COMMENT OUT WHAT YOU'RE DOING PLEASE <3
/// SO THAT WAY WHEN I GET HUNDREDS OF MERGE CONFLICTS I KNOW WHATS NEEDED AND WHATS NOT <3
/// </im not mad the code just was spaghetti when i got here>


public class Class_Mage : MonoBehaviour
{
    PlayerController player;

    bool holdingSecondary;
    bool fireSpraying;

    [SerializeField] float primaryStamCost = 0.05f;  
    [SerializeField] float secondaryStamCost = 0.5f;
    [SerializeField] float secondaryFireSpeed = .5f;


    //this is our start function that does a few important things
    private void Start()
    {
        //first we find our player and save him as an easily accessible variable
        player = GameManager.instance.player.GetComponent<PlayerController>();
        //next we set our fire particle system to active and turn it off so we can toggle it easier later
        player.combatObjects[1].SetActive(true);
        player.combatObjects[1].GetComponent<ParticleSystem>().Stop();
    }

    private void Update()
    {
        if (holdingSecondary) 
        {
            StartCoroutine(SummonFireSpray());
        }
    }

    //i imagine this gets called when the input associated with primary fire is triggered
    //i could not tell you what ctxt is or what information it holds or its importance though
    //i also have no idea why this function is public, that seems important though
    public void OnPrimaryFire(InputAction.CallbackContext ctxt)
    {
        Debug.Log("Context is performed");
        Debug.Log(ctxt.performed);
        Debug.Log("Attack is Valid");
        Debug.Log(ValidAttack());
        Debug.Log("Stamina is Valid");
        Debug.Log(StaminaCheck(primaryStamCost));
        //checks for if our input was performed and if its valid to attack
        //check the ValidAttack Function to see what quantifies as valid
        //also checks if we have the stamina to attack
        if (ctxt.performed && ValidAttack() && StaminaCheck(primaryStamCost))
        {
            GameManager.instance.isShooting = true;
            player.currentStamina -= primaryStamCost;
            //starts our mage primary attack animation and plays our associated sound
            player.SetAnimationTrigger("Mage1");
            player.PlaySound('A');
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
            Debug.Log("No Staminaaaaaa :(");
        }
    }

    //the same as OnPrimaryFire but for our secondary attack
    public void OnSecondaryFire(InputAction.CallbackContext ctxt)
    {
        //Checks the same things as primary attack
        if (ctxt.performed && ValidAttack() && StaminaCheck(secondaryStamCost))
        {
            GameManager.instance.isShooting = true;
            player.currentStamina -= secondaryStamCost;
            //sets our bool animator bool to true to start the animation and plays our associated audio
            player.SetAnimationBool("Mage2", true);
            player.PlaySound('A');
            //turns on our particle system and starts the coroutine that summons the damaging projectiles of our attack
            player.combatObjects[1].GetComponent<ParticleSystem>().Play();
            holdingSecondary = true;
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

            Debug.Log("No Staminaaaaaa :(");
        }
        //if we stop holding the input this code runs
        else if (ctxt.canceled && GameManager.instance.isShooting)
        {
            //sets us to not attacking, sets our animation bool to false so we can end the animation, and stops our particle system and coroutine
            GameManager.instance.isShooting = false;
            player.SetAnimationBool("Mage2", false);
            player.combatObjects[1].GetComponent<ParticleSystem>().Stop(true, ParticleSystemStopBehavior.StopEmitting);
            holdingSecondary = false;
        }
    }

    //This function is called as an animation event and it will summon our fireball at the appropriate time during the animation
    void PrimaryFire()
    {
        //spawns our projectile either locally or in all lobbies depending on whether your playing solo or multiplayer
        if (PhotonNetwork.InRoom)
             PhotonNetwork.Instantiate(player.combatObjects[0].name, player.shootPosition.transform.position, player.shootPosition.transform.rotation);
        else if (!PhotonNetwork.InRoom)
             Instantiate(player.combatObjects[0], player.shootPosition.transform.position, player.shootPosition.transform.rotation); GameManager.instance.isShooting = false;
    }

    //this function is for stopping the fire spray attack if you run out of stamina
    void EndSecondary()
    {
        //sets us to not attacking, sets our animation bool to false so we can end the animation, and stops our particle system and coroutine
        GameManager.instance.isShooting = false;
        player.SetAnimationBool("Mage2", false);
        player.combatObjects[1].GetComponent<ParticleSystem>().Stop(true, ParticleSystemStopBehavior.StopEmitting);
        holdingSecondary = false;
        // Checking for audio ( preventing looping on sounds )
        if (!player.staminaAudioSource.isPlaying)
        {
            // Play out of stamina sound
            player.staminaAudioSource.PlayOneShot(player.noAttack[Random.Range(0, player.noAttack.Length)], player.noAttackVol);
            player.isPlayingStamina = true;
        }

        player.isPlayingStamina = player.staminaAudioSource.isPlaying;

        Debug.Log("No Staminaaaaaa :(");
    }

    //coroutine that takes in our adjustable timing and only summons a damage projectile every so often
    IEnumerator SummonFireSpray()
    {
        if(!StaminaCheck(secondaryStamCost))
        {
            EndSecondary();
        }
        else if(!fireSpraying)
        {
            fireSpraying = true;
            player.currentStamina -= secondaryStamCost;
            //summons either locally or for all connected game instances
            if (PhotonNetwork.InRoom)
                PhotonNetwork.Instantiate(player.combatObjects[2].name, player.shootPosition.transform.position, player.shootPosition.transform.rotation);
            else if (!PhotonNetwork.InRoom)
                Instantiate(player.combatObjects[2], player.shootPosition.transform.position, player.shootPosition.transform.rotation);
            //waits
            yield return new WaitForSeconds(secondaryFireSpeed);
            fireSpraying = false;
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
        if (player.currentStamina >= staminaRequired)
        {
            return true;
        }
        return false;
    }
}
