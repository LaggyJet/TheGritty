//Worked on By : Jacob Irvin, Joshua Furber
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class Class_Warrior : MonoBehaviour
{
    PlayerController player;
    MeleeWeaponController weapon;

    bool holdingSecondary;
    bool waiting;
    bool isCounting;
    int abilityCoolDown;
    int abilityActive;

    float primaryStamCost = 0.3f;
    float secondaryStamCost = 0.025f;
    float secondaryTickSpeed = .5f;
    float abilityStamCost = 1.0f;

    float damage = 10;
    bool canDOT = false;
    float abilityMultiplier = 2f;
    bool staminaUnlockedCheck, attackSpeedUnlockedCheck, ability1UnlockedCheck, ability2UnlockedCheck, ability3UnlockedCheck = false;

    //this is our start function that does a few important things
    private void Start()
    {
        //first we find our player and save him as an easily accessible variable and adds a component to our weapon
        player = GetComponent<PlayerController>();
        player.combatObjects[3].AddComponent<MeleeWeaponController>().SetWeapon(damage, canDOT, null);
        weapon = player.combatObjects[3].GetComponent<MeleeWeaponController>();
        //next we set our fire particle system to active and turn it off so we can toggle it easier later
        player.combatObjects[3].SetActive(true);
        player.combatObjects[4].SetActive(true);
    }

    private void Update()
    {
        // Check if the player has the stamina use down unlocked and prevent repeat calls if they do
        if (!staminaUnlockedCheck && SkillTreeManager.Instance.IsSkillUnlocked(SkillTreeManager.Skills.STAMINA_USE_DOWN)) {
            primaryStamCost = 0.2f;
            secondaryStamCost = 0.02f;
            abilityStamCost = 0.7f;
            staminaUnlockedCheck = true;
        }

        // Check if the player has the attack speed up unlocked and prevent repeat calls if they do
        if (!attackSpeedUnlockedCheck && SkillTreeManager.Instance.IsSkillUnlocked(SkillTreeManager.Skills.ATTACK_SPEED_UP))
        {
            player.SetAnimationSpeed(0.75f);
            attackSpeedUnlockedCheck = true;
        }

        // Check if the player has the ability strength 1 unlocked and prevent repeat calls if they do
        if (!ability1UnlockedCheck && SkillTreeManager.Instance.IsSkillUnlocked(SkillTreeManager.Skills.ABILITY_STRENGTH_1)) {
            abilityMultiplier = 2.25f;
            ability1UnlockedCheck = true;
        }
        // Check if the player has the ability strength 2 unlocked and prevent repeat calls if they do
        else if (!ability2UnlockedCheck && SkillTreeManager.Instance.IsSkillUnlocked(SkillTreeManager.Skills.ABILITY_STRENGTH_2)) {
            abilityMultiplier = 2.5f;
            ability2UnlockedCheck = true;
        }
        // Check if the player has the ability strength 3 unlocked and prevent repeat calls if they do
        else if (!ability3UnlockedCheck && SkillTreeManager.Instance.IsSkillUnlocked(SkillTreeManager.Skills.ABILITY_STRENGTH_3)) {
            abilityMultiplier = 3f;
            ability3UnlockedCheck = true;
        }

        if (abilityActive > 0)
        {
            StartCoroutine(AbilityActive());
        }
        else if (abilityCoolDown > 0)
        {
            StartCoroutine(AbilityCountDown());
        }
        if (holdingSecondary)
        {
            StartCoroutine(BlockingStaminaUse());
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
            player.SetAnimationTrigger("Warrior1");
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
            player.SetAnimationBool("Warrior2", true);
            holdingSecondary = true;
            player.isBlocking = true;
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
            player.SetAnimationBool("Warrior2", false);
            holdingSecondary = false;
            player.isBlocking = false;
        }
    }

    IEnumerator BlockingStaminaUse()
    {
        if (!StaminaCheck(secondaryStamCost))
        {
            EndSecondary();
        }
        else if(!waiting)
        {
            waiting = true;
            yield return new WaitForSeconds(secondaryTickSpeed);
            player.stamina -= secondaryStamCost;
            waiting = false;
        }
    }

    
    public void OnAbility(InputAction.CallbackContext ctxt)
    {
        //checks that we are not on cooldown and not using the ability
        if (ctxt.performed && ValidAttack() && StaminaCheck(abilityStamCost) && abilityCoolDown == 0 && abilityActive == 0)
        {
            //plays our animation and sound for the ability
            player.SetAnimationTrigger("Warrior3");
            weapon.damage_ *= abilityMultiplier;        
            player.PlaySound('A');

            abilityActive = 5;
            abilityCoolDown = 15;
        }
    }

    //function for counting down our active time
    IEnumerator AbilityActive()
    {
        if (!isCounting)
        {
            isCounting = true;
            yield return new WaitForSeconds(1);
            abilityActive--;
            isCounting = false;
        }
        if(abilityActive == 0)
        {
            weapon.damage_ /= 2;
        }
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

    //this function is for stopping the fire spray attack if you run out of stamina
    void EndSecondary()
    {
        //sets us to not attacking, sets our animation bool to false so we can end the animation, and stops our coroutine
        GameManager.instance.isShooting = false;
        player.SetAnimationBool("Warrior2", false);
        holdingSecondary = false;
        player.isBlocking = false;
        // Checking for audio ( preventing looping on sounds )
        if (!player.staminaAudioSource.isPlaying)
        {
            // Play out of stamina sound
            player.staminaAudioSource.PlayOneShot(player.noAttack[Random.Range(0, player.noAttack.Length)], player.noAttackVol);
            player.isPlayingStamina = true;
        }
        player.isPlayingStamina = player.staminaAudioSource.isPlaying;
    }


    public void SwingStart()
    {
        player.combatObjects[3].GetComponent<Collider>().enabled = true;
    }
    public void SwingEnd()
    {
        player.combatObjects[3].GetComponent<Collider>().enabled = false;
        GameManager.instance.isShooting = false;
        player.combatObjects[3].GetComponent<MeleeWeaponController>().didDamage = false;
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
