//Worked on By : Jacob Irvin, Joshua Furber

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using Photon.Pun;

public class Class_Mage : MonoBehaviourPun
{
    PlayerController player;

    bool holdingSecondary;
    bool fireSpraying;
    bool isCounting;
    int abilityCoolDown;
    int abilityActive;

    float primaryStamCost = 0.05f;  
    float secondaryStamCost = 0.35f;
    float secondaryFireSpeed = .2f;

    int abilityActiveTime = 3;
    bool staminaUnlockedCheck, ability1UnlockedCheck, ability2UnlockedCheck, ability3UnlockedCheck = false;

    //this is our start function that does a few important things
    private void Start()
    {
        //first we find our player and save him as an easily accessible variable
        player = GetComponent<PlayerController>();

        // Set the particle system false for activation later
        // Checks for if fire belongs to that player, or they are in singleplayer
        if (photonView.IsMine || !PhotonNetwork.IsConnected)
            player.combatObjects[1].SetActive(false);
    }

    private void Update()
    {
        if (!staminaUnlockedCheck && SkillTreeManager.Instance.IsSkillUnlocked(SkillTreeManager.Skills.STAMINA_USE_DOWN)) {
            primaryStamCost = 0.03f;
            secondaryStamCost = 0.2f;
        }

        if (!ability1UnlockedCheck && SkillTreeManager.Instance.IsSkillUnlocked(SkillTreeManager.Skills.ABILITY_STRENGTH_1))
        {
            abilityActiveTime = 4;
            ability1UnlockedCheck = true;
        }
        else if (!ability2UnlockedCheck && SkillTreeManager.Instance.IsSkillUnlocked(SkillTreeManager.Skills.ABILITY_STRENGTH_2))
        {
            abilityActiveTime = 5;
            ability2UnlockedCheck = true;
        }
        else if (!ability3UnlockedCheck && SkillTreeManager.Instance.IsSkillUnlocked(SkillTreeManager.Skills.ABILITY_STRENGTH_3))
        {
            abilityActiveTime = 6;
            ability3UnlockedCheck = true;
        }

        if (abilityActive > 0)
        {
            StartCoroutine(AbilityActive());
        }
        else if(abilityCoolDown > 0)
        {
            StartCoroutine(AbilityCountDown());
        }
        if (holdingSecondary) 
        {
            StartCoroutine(SummonFireSpray());
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
            if(player.useStamina)
                player.stamina -= primaryStamCost;
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
        }
    }

    //the same as OnPrimaryFire but for our secondary attack
    public void OnSecondaryFire(InputAction.CallbackContext ctxt)
    {
        //Checks the same things as primary attack
        if (ctxt.performed && ValidAttack() && StaminaCheck(secondaryStamCost))
        {
            GameManager.instance.isShooting = true;
            //sets our bool animator bool to true to start the animation and plays our associated audio
            player.SetAnimationBool("Mage2", true);
            player.PlaySound('A');
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
        }
        //if we stop holding the input this code runs
        else if (ctxt.canceled && GameManager.instance.isShooting)
        {
            //sets us to not attacking, sets our animation bool to false so we can end the animation, and stops our particle system and coroutine
            GameManager.instance.isShooting = false;
            player.SetAnimationBool("Mage2", false);

            //If in multiplayer, call via RPC, otherwise, call normally
            if (PhotonNetwork.InRoom)
                photonView.RPC(nameof(StopParticles), RpcTarget.All, photonView.ViewID);
            else if (!PhotonNetwork.IsConnected)
                StopParticles(-1);

            holdingSecondary = false;
        }
    }

    //function for using our class ability, which gives us max stamina and makes us not use stamina for the next three seconds
    public void OnAbility(InputAction.CallbackContext ctxt)
    {
        //checks that we are not on cooldown and not using the ability
        if (ctxt.performed && ValidAttack() && abilityCoolDown == 0 && abilityActive == 0)
        {
            //plays our animation and sound for the ability
            player.SetAnimationTrigger("Mage3");
            player.PlaySound('A');
            //sets our active time to 3 and our cooldown time to 10
            abilityActive = abilityActiveTime;
            abilityCoolDown = 10;
            //makes stamina maximum so we pass all stamina checks during our ability time
            player.stamina = 10;
        }
    }

    //function for counting down our active time
    IEnumerator AbilityActive()
    {
        if(!isCounting)
        {
            if(player.useStamina)
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
            if(!player.useStamina)
            {
                player.useStamina = true;
            }
            isCounting = true;
            yield return new WaitForSeconds(1);
            abilityCoolDown--;
            isCounting = false;
        }
    }


    //This function is called as an animation event and it will summon our fireball at the appropriate time during the animation
    void PrimaryFire()
    {
        //spawns our projectile either locally or in all lobbies depending on whether your playing solo or multiplayer
        if (PhotonNetwork.InRoom && photonView.IsMine)
            PhotonNetwork.Instantiate("Player/" + player.combatObjects[0].name, player.shootPosition.transform.position, player.shootPosition.transform.rotation);
        else if (!PhotonNetwork.IsConnected)
            Instantiate(player.combatObjects[0], player.shootPosition.transform.position, player.shootPosition.transform.rotation); GameManager.instance.isShooting = false;
    }

    // Backup stop for singleplayer
    void StopFireParticles() { player.combatObjects[1].SetActive(false); }

    // Animation event call
    void StartParticles()
    {
        //turns on our particle system
        player.combatObjects[1].SetActive(true);
        
        // Have particles display for other parties if in multiplayer
        if (PhotonNetwork.InRoom)
            photonView.RPC(nameof(StartParticles), RpcTarget.All, photonView.ViewID);
    }

    // RPC calls to sync the particle system
    [PunRPC]
    void StartParticles(int viewID) {
        // Get all current players and loop through them
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject plr in players)
            // Checks if that player is shooting fire, if so, enable it for the user
            if (plr.GetComponent<PhotonView>().ViewID == viewID)
                plr.GetComponent<PlayerController>().combatObjects[1].SetActive(true);
    }

    [PunRPC]
    void StopParticles(int viewID) {
        // Disable normally for the own player
        if (photonView.IsMine || viewID == -1) {
            player.combatObjects[1].GetComponent<ParticleSystem>().Stop(true, ParticleSystemStopBehavior.StopEmitting);
            player.combatObjects[1].SetActive(false);
        }
        // Otherwise, repeat similar steps as above RPC call, and disable instead
        else
        {
            GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
            foreach (GameObject plr in players) {
                if (plr.GetComponent<PhotonView>().ViewID == viewID) {
                    plr.GetComponent<PlayerController>().combatObjects[1].GetComponent<ParticleSystem>().Stop(true, ParticleSystemStopBehavior.StopEmitting);
                    plr.GetComponent<PlayerController>().combatObjects[1].SetActive(false);
                }
            }
        }
    }
    // End RPC calls

    //this function is for stopping the fire spray attack if you run out of stamina
    void EndSecondary()
    {
        //sets us to not attacking, sets our animation bool to false so we can end the animation, and stops our particle system and coroutine
        GameManager.instance.isShooting = false;
        player.SetAnimationBool("Mage2", false);

        //If in multiplayer, call via RPC, otherwise, call normally
        if (PhotonNetwork.InRoom)
            photonView.RPC(nameof(StopParticles), RpcTarget.All, photonView.ViewID);
        else if (!PhotonNetwork.IsConnected)
            StopParticles(-1);

        holdingSecondary = false;
        // Checking for audio ( preventing looping on sounds )
        if (!player.staminaAudioSource.isPlaying)
        {
            // Play out of stamina sound
            player.staminaAudioSource.PlayOneShot(player.noAttack[Random.Range(0, player.noAttack.Length)], player.noAttackVol);
            player.isPlayingStamina = true;
        }
        player.isPlayingStamina = player.staminaAudioSource.isPlaying;
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
            if (player.useStamina)
                player.stamina -= secondaryStamCost;
            //summons either locally or for all connected game instances
            if (PhotonNetwork.InRoom)
                PhotonNetwork.Instantiate("Player/" + player.combatObjects[2].name, player.shootPosition.transform.position, player.shootPosition.transform.rotation);
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
        if (player.stamina >= staminaRequired)
        {
            return true;
        }
        return false;
    }
}
