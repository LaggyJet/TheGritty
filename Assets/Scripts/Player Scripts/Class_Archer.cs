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
    bool isCounting = false;
    int abilityCoolDown = 0;
    float dashMultipler = 30f;
    bool staminaUnlockedCheck, ability1UnlockedCheck, ability2UnlockedCheck, ability3UnlockedCheck = false;

    // Start is called before the first frame update
    void Start()
    {
        // Set the player set and save for access later
        player = GetComponent<PlayerController>();

        player.combatObjects[6].SetActive(true);
    }

    private void Update()
    {
        // Check if the player has the stamina use down unlocked and prevent repeat calls if they do
        if (!staminaUnlockedCheck && SkillTreeManager.Instance.IsSkillUnlocked(SkillTreeManager.Skills.STAMINA_USE_DOWN)) {
            primaryStamCost = 0.2f;
            secondaryStamCost = 0.2f;
            abilityStamCost = 1f;
        }

        // Check if the player has the ability strength 1 unlocked and prevent repeat calls if they do
        if (!ability1UnlockedCheck && SkillTreeManager.Instance.IsSkillUnlocked(SkillTreeManager.Skills.ABILITY_STRENGTH_1))
        {
            dashMultipler = 35f;
            ability1UnlockedCheck = true;
        }
        // Check if the player has the ability strength 2 unlocked and prevent repeat calls if they do
        else if (!ability2UnlockedCheck && SkillTreeManager.Instance.IsSkillUnlocked(SkillTreeManager.Skills.ABILITY_STRENGTH_2))
        {
            dashMultipler = 40f;
            ability2UnlockedCheck = true;
        }
        // Check if the player has the ability strength 3 unlocked and prevent repeat calls if they do
        else if (!ability3UnlockedCheck && SkillTreeManager.Instance.IsSkillUnlocked(SkillTreeManager.Skills.ABILITY_STRENGTH_3))
        {
            dashMultipler = 45f;
            ability3UnlockedCheck = true;
        }

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

        float dashDuration = 0.25f; 
        float speedUpDuration = 0.075f;
        float slowDownDuration = 0.05f;
        float dashSpeed = dashMultipler;
        float timeElapsed = 0f;
        float normalSpeed = player.speed;

        while (timeElapsed < speedUpDuration) {
            player.controller.Move(Mathf.Lerp(normalSpeed, dashSpeed, (timeElapsed / speedUpDuration)) * Time.deltaTime * player.movement);
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        timeElapsed = 0f;
        while (timeElapsed < dashDuration - (speedUpDuration + slowDownDuration)) {
            player.controller.Move(dashSpeed * Time.deltaTime * player.movement);
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        timeElapsed = 0f;
        while (timeElapsed < slowDownDuration) {
            player.controller.Move(Mathf.Lerp(dashSpeed, normalSpeed, (timeElapsed / slowDownDuration)) * Time.deltaTime * player.movement);
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        player.controller.Move(normalSpeed * Time.deltaTime * player.movement);
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
        Vector3 pos = player.arrowPosition.transform.position;
        Quaternion rot = player.transform.rotation;

        float[] angles = { -5, 0, 5 };
        for (int i = 0; i < angles.Length; i++) {
            if (PhotonNetwork.InRoom && photonView.IsMine)
                PhotonNetwork.Instantiate("Player/" + player.combatObjects[5].name, pos, (rot * Quaternion.Euler(0, angles[i], 0)));
            else if (!PhotonNetwork.IsConnected)
                Instantiate(player.combatObjects[5], pos, (rot * Quaternion.Euler(0, angles[i], 0)));
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
