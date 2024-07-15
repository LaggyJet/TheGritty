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


    float primaryStamCost = 0.3f;
    float secondaryStamCost = 0.3f;
    float abilityStamCost = 1.5f;
    float dashSpeed = 125f;

    bool isCounting = false;
    int abilityCoolDown = 0;

    // Start is called before the first frame update
    void Start()
    {
        // Set the player set and save for access later
        player = GetComponent<PlayerController>();

        player.combatObjects[6].SetActive(true);
    }

    private void Update()
    {
        if (abilityCoolDown > 0)
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
            player.stamina -= primaryStamCost;  
            //starts our mage primary attack animation and plays our associated sound
            player.SetAnimationTrigger("Archer1");
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
        }
    }
    public void OnSecondaryFire(InputAction.CallbackContext ctxt)
    {
        //checks for if our input was performed and if its valid to attack
        //check the ValidAttack Function to see what quantifies as valid
        //also checks if we have the stamina to attack
        if (ctxt.performed && ValidAttack() && StaminaCheck(secondaryStamCost))
        {
            GameManager.instance.isShooting = true;
            player.stamina -= secondaryStamCost;
            //starts our mage primary attack animation and plays our associated sound
            player.SetAnimationTrigger("Archer2");
            player.PlaySound('A');
        }
        //checks for if the input was performed in a valid context but there isn't enough stamina to perform our action
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
    }

    public void OnAbility(InputAction.CallbackContext ctxt)
    {
        
        //checks that we are not on cooldown and not using the ability
        if (ctxt.performed && ValidAttack() && StaminaCheck(abilityStamCost) && abilityCoolDown == 0)
        {
            StartCoroutine(DashLength());

            abilityCoolDown = 2;
        }
    }

    IEnumerator DashLength() {
        PlayerController player = GetComponent<PlayerController>();
        player.controller.Move((player.speed * dashSpeed) * Time.deltaTime * player.movement);
        yield return new WaitForSeconds(1);
        player.controller.Move(dashSpeed * Time.deltaTime * player.movement);
    }

    //function for counting down our cooldown
    IEnumerator AbilityCountDown()
    {
        if (!isCounting)
        {
            isCounting = true;
            yield return new WaitForSeconds(1);
            abilityCoolDown--;
            isCounting = false;
        }
    }


    void QuickShot()
    {
        if (PhotonNetwork.InRoom && photonView.IsMine)
        {
            PhotonNetwork.Instantiate("Player/" + player.combatObjects[5].name, player.arrowPosition.transform.position, player.transform.rotation);
        }
        // Otherwise spawn regularly 
        else if (!PhotonNetwork.IsConnected)
        {
            Instantiate(player.combatObjects[5], player.arrowPosition.transform.position, player.transform.rotation);
        }

        GameManager.instance.isShooting = false;
    }

    void TripleShot()
    {
        if (PhotonNetwork.InRoom && photonView.IsMine)
        {
            Vector3 pos = player.arrowPosition.transform.position;
            Quaternion rot = player.transform.rotation;
            PhotonNetwork.Instantiate("Player/" + player.combatObjects[5].name, pos, rot);
            pos.x -= .5f;
            pos.y -= .5f;
            PhotonNetwork.Instantiate("Player/" + player.combatObjects[5].name, pos, rot);
            pos.x += 1f;
            PhotonNetwork.Instantiate("Player/" + player.combatObjects[5].name, pos, rot);
        }
        // Otherwise spawn regularly 
        else if (!PhotonNetwork.IsConnected)
        {
            Quaternion rot = player.transform.rotation;
            for (int i = 1; i <= 3; i++) {
                rot.x = i * 15;
                Instantiate(player.combatObjects[5], player.arrowPosition.transform.position, rot);
            }
        }

        GameManager.instance.isShooting = false;
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
