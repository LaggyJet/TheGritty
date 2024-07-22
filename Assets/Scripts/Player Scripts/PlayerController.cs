//Worked on by : Jacob Irvin, Natalie Lubahn, Kheera, Emily Underwood, Joshua Furber

using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using static UnityEngine.GraphicsBuffer;

public class PlayerController : MonoBehaviourPunCallbacks, IDamage, IDataPersistence
{
    //lets us access player and controller and character controller from outside the object
    public static PlayerController instance; 
    public CharacterController controller;

    [SerializeField] Animator animate;

    [Header("------- Movement and Position -------")]
    [SerializeField] public float speed;
    [SerializeField] int sprintMod;
    [SerializeField] int gravity;
    [SerializeField] int jumpMax;
    [SerializeField] int jumpSpeed;
    int jumpsAvailable;
    Vector3 moveDir;
    public Vector3 playerV, movement;
    public static Vector3 spawnLocation;
    public static Quaternion spawnRotation;


    [Header("------- HP -------")]
    DamageStats status;
    public bool isDead = false;
    bool isDOT;
    public static float spawnHP;
    // Health bar colors and script for shaking the ui
    [Range(0f, 10f)] public float hp; 
    public float hpBase; 
    public float HpDisplay;
    public bool aboveThresholdHP;


    // Health bar gradual fill 
    [SerializeField] Color fullHealth; 
    [SerializeField] Color midHealth; 
    [SerializeField] Color criticalHealth;
    [SerializeField] public Shake hpShake;
    


    [Header("------- Stamina -------")]
    [Range(0f, 10f)] public float stamina;
    public float staminaBase;
    public static float spawnStamina;
    public float StaminaDisplay;
    public bool abovethresholdSTAM;

    [Range(0f, 50f)] public float staminaRegenerate;  
    
    
     // stamina bar gradual fill 
    [SerializeField] Color fullStamina; 
    [SerializeField] Color midStamina; 
    [SerializeField] Color criticalStamina;
    [SerializeField] public Shake stamShake; 

    [Header("------- Combat -------")]
    [SerializeField] public GameObject shootPosition;
    [SerializeField] public GameObject arrowPosition;
    [SerializeField] public GameObject[] combatObjects;
    [SerializeField] public GameObject targetPosition, headPosition;
    ClassSelection classSelector;
    public bool useStamina = true;
    public bool isBlocking = false;
    public int classCase;
    public Class_Mage mage = null;
    public Class_Warrior warrior = null;
    public Class_Archer archer = null;
    //these are animation variables
    [SerializeField] float animationTransSpeed;
    [Range(0f, 10f)] public float lerpSpeed;
    


    [Header("------ Audio ------")]
    [SerializeField] public AudioSource audioSource;
    [SerializeField] AudioClip[] footsteps;
    [SerializeField] float footstepsVol;
    [SerializeField] AudioClip[] hurt;
    [SerializeField] float hurtVol;
    [SerializeField] public AudioClip[] attack;
    [SerializeField] float attackVol;
    bool isPlayingSteps;

    [Header("-----Player SFX------")]
    [SerializeField] public AudioClip playerJump;
    [SerializeField] public float playerJumpVol;
    public bool isPlayingJump = false;
    [SerializeField] public AudioClip playerLand;
    [SerializeField] public float playerLandVol;
    public bool isPlayingLand = false;
    [SerializeField] public AudioClip playerTeleport;
    [SerializeField] public float playerTeleportVol;
    public bool isPlayingTeleport = false;
    [SerializeField] public AudioClip[] WeaponHit;
    [SerializeField] public float weaponHitVol;
    public bool isPlayingWeaponHit = false;
    [SerializeField] public AudioClip[] arrowAud;
    [SerializeField] public float arrowAudVol;
    public bool isPlayingArrowAud = false;
    [SerializeField] public AudioClip[] swordHitAud;
    [SerializeField] public float swordHitAudVol;
    public bool isPlayingSwordHitAud = false;
    [SerializeField] public AudioClip[] arrowHitAud;
    [SerializeField] public float arrowHitAudVol;
    public bool isPlayingArrowHitAud = false;
    [SerializeField] public AudioClip[] playerDeadAud;
    [SerializeField] public float playerDeadAudVol;
    public bool isPlayingPlayerDeadAud = false;
    [SerializeField] public AudioClip unlockSkillAud;
    [SerializeField] public float unlockSkillAudVol;
    public bool isPlayingUnlockSkillAud = false;
    [SerializeField] public AudioClip CantUnlockSkillAud;
    [SerializeField] public float cantUnlockSkillAudVol;
    public bool isPlayingCantUnlockSkillAud = false;
    [SerializeField] public AudioClip losePointAud;
    [SerializeField] public float losePointAudVol;
    public bool isPlayingLosePointAud = false;
    [SerializeField] public AudioClip addPointAud;
    [SerializeField] public float addPointAudVol;
    public bool isPlayingAddPointAud = false;

    [Header("------ Map Audio ------")]
    [SerializeField] public AudioClip[] doorCloseAud;
    [SerializeField] public float doorCloseAudVol;
    public bool isPlayingDoorCloseAud = false;
    [SerializeField] public AudioClip[] doorOpenAud;
    [SerializeField] public float doorOpenAudVol;
    public bool isPlayingDoorOpenAud = false;

    [Header("------ Enemy Audio ------")]
    [SerializeField] public AudioClip[] collsionAud;
    [SerializeField] public float collisionAudVol;
    public bool isPlayingCollisionAud = false;
    [SerializeField] public AudioClip[] iceEnemyAud;
    [SerializeField] public float iceEnemyAudVol;
    public bool isPlayingIceEnemyAud = false;
    [SerializeField] public AudioClip iceDeathAud;
    [SerializeField] public float iceDeathAudVol;
    public bool isPlayingIceDeathAud = false;
    [SerializeField] public AudioClip[] skeletonAud;
    [SerializeField] public float skeletonAudVol;
    public bool isPlayingskeletonAud = false;




    [Header("------ Sprint Audio ------")]
    [SerializeField] public AudioSource sprintAudioSource;
    [SerializeField] public AudioClip sprintSound;
    [SerializeField] float sprintVol;
    [SerializeField] public AudioClip[] noSprint;
    [SerializeField] float noSprintVol;
    bool isSprinting;
    public bool isPlayingStamina;
    public bool isPlayingNoSprinting;


    [Header("------ Stamina/HP Audio ------")]
    [SerializeField] public AudioSource staminaAudioSource;
    [SerializeField] public AudioClip[] noHP;
    [SerializeField] public float noHPvol;
    [SerializeField] public AudioClip[] noAttack;
    [SerializeField] public float noAttackVol; 
    public bool isPlayingNoHP = false;
    private bool isRegenerating = false;

    
    private const string CLASS_SELECTED = "ClassSelected";
    ClassSelection currentClassSelection;

    // Skill tree vars
    float hpBuff = 15f;
    float damageReduction = 1f;
    bool hasShield = false;
    int timeUntilShieldRegen = 0;
    int MAX_REGEN_TIME = 5;
    bool hpAmountUnlockedCheck, damageReductionUnlockedCheck, shieldUnlockedCheck = false;

    private void Awake()
    {
        instance = this;
    }

    private void Start() {
        // Get base selection (if possible)
        currentClassSelection = GameObject.FindWithTag("ClassSelector")?.GetComponent<ClassSelection>();

        // Setting up class for yourself or multiplayer
        if (PhotonNetwork.InRoom || !PhotonNetwork.InRoom) {
            int selectedClass = currentClassSelection.MyClass;
            AssignClass(selectedClass);

            // If multiplayer, update the localplayer's hash table for other clients
            if (PhotonNetwork.InRoom && photonView.IsMine) {
                ExitGames.Client.Photon.Hashtable properties = new ExitGames.Client.Photon.Hashtable();
                properties[CLASS_SELECTED] = selectedClass;
                PhotonNetwork.LocalPlayer.SetCustomProperties(properties);
            }
        }

        // Prevents your inputs from moving other players in multiplayer
        // Checks if the player is the actual player and in multiplayer
        // Also assigns the proper class to other player on your end
        if (!photonView.IsMine && PhotonNetwork.InRoom) {
            // Prevent 2 instances of camera/audio listener
            Destroy(GetComponentInChildren<Camera>().gameObject);
            Destroy(GetComponentInChildren<AudioListener>());

            //Make other player visible
            UpdateOtherPlayer(gameObject);

            // Update local player class on server to update on other clients
            if (photonView.Owner.CustomProperties.TryGetValue(CLASS_SELECTED, out object classSelection)) {
                int newClassSelection = (int)classSelection;
                if (newClassSelection != currentClassSelection.MyClass) {
                    AssignClass(newClassSelection);
                    currentClassSelection.MyClass = newClassSelection;
                }
            }
        }


        //tracks our base currentHP and the current currentHP that will update as our player takes damage or gets health
        hpBase = hp;
        staminaBase = stamina;
        transform.position = Vector3.zero;
        transform.rotation = Quaternion.identity;

        // For smooth Transition UI - not the same as above 
        HpDisplay = hp / hpBase;
        StaminaDisplay = stamina / staminaBase;

        // Keeps track for sounds manager
        aboveThresholdHP = HpDisplay > 0.5f;
        abovethresholdSTAM = StaminaDisplay > 0.5f;

        //calls a function to set the player variable in the game manager
        GameManager.instance.SetPlayer();

        //this is our spawn function
        if (spawnLocation == Vector3.zero) //checks if the spawnLocation is a vector 3 zero, meaning it is a new game
        {
            // Todo: Might need update player UI 
            transform.position = GameManager.playerLocation;
            transform.rotation = GameManager.instance.player.transform.rotation ;
        }
        else //otherwise the spawnLocation has a value and means the game is being resumed
        {
            GameManager.playerLocation = spawnLocation;
            transform.position = spawnLocation;
            transform.rotation = spawnRotation;
            hp = spawnHP;
            stamina = spawnStamina;
            Physics.SyncTransforms();
            // Todo: Might need update player UI 
            spawnLocation = Vector3.zero;
        }
    }

    void Update()
    {
        // Check if the player has the shield unlocked and prevent repeat calls if they do
        if (!shieldUnlockedCheck && SkillTreeManager.Instance.IsSkillUnlocked(SkillTreeManager.Skills.SHIELD))
            shieldUnlockedCheck = true;

        // Regen the shield if timer is over
        if (!hasShield && shieldUnlockedCheck && timeUntilShieldRegen == 0)
            hasShield = true;

        // Check if the player has the damage taken down unlocked and prevent repeat calls if they do
        if (!damageReductionUnlockedCheck && SkillTreeManager.Instance.IsSkillUnlocked(SkillTreeManager.Skills.DAMAGE_TAKEN_DOWN)) {
            damageReductionUnlockedCheck = true;
            damageReduction = 0.75f;
        }

        // Check if the player has the hp amount up unlocked and prevent repeat calls if they do
        if (!hpAmountUnlockedCheck && SkillTreeManager.Instance.IsSkillUnlocked(SkillTreeManager.Skills.HP_AMOUNT_UP)) {
            hpAmountUnlockedCheck = true;
            hpBase = hp = hpBuff;
        }


        // Make sure the player keeps their class in multiplayer
        if (photonView.IsMine)
            VerifyClassState();

        //if these are currently null we search for them every frame until we find them
        if (stamShake == null || hpShake == null)
        {
            stamShake = GameManager.instance.staminaBar.GetComponent<Shake>();
            hpShake = GameManager.instance.playerHPBar.GetComponent<Shake>();
        }

        // Prevent movement of other players
        if (!PhotonNetwork.InRoom || GetComponent<PhotonView>().IsMine)
        {
            //runs our movement function to determine the player velocity each frame
            Movement();
            // Regenerating over time ( can be adjusted in unity )
            RegenerateStamina();
            //updates our ui every frame
            UpdatePlayerUI();
        }
    }

    // Reset the children of the other player to be on default layer to be seen by camera of current player
    void UpdateOtherPlayer(GameObject obj) {
        if (!obj) return;

        obj.layer = 0;
        foreach (Transform child in obj.transform) {
            if (!child) return;
            UpdateOtherPlayer(child.gameObject);
        }
    }

    // Deals with character selection in multiplayer
    public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps) {
        if (changedProps.ContainsKey(CLASS_SELECTED) && targetPlayer == photonView.Owner) {
            int newClassSelection = (int)changedProps[CLASS_SELECTED];
            if (currentClassSelection != null && newClassSelection != currentClassSelection.MyClass) {
                AssignClass(newClassSelection);
                currentClassSelection.MyClass = newClassSelection;
            }
        }
    }

    //methods for the controls that are utilized in the Player input map to control the player
    public void OnMove(InputAction.CallbackContext ctxt) //moving
    {
        Vector2 newMoveDir = ctxt.ReadValue<Vector2>();
        moveDir.x = newMoveDir.x;
        moveDir.z = newMoveDir.y;
    }

    public void OnJump(InputAction.CallbackContext ctxt) //jumping
    {
        //checks if our input is called, if we have available jumps left, and if we have the stamina to jump
        if (ctxt.performed && jumpsAvailable > 0 && stamina >= .5f)
        {
            PlayJump();
            //subtracts our stamina and jumps then adds our y velocity to player
            stamina -= .5f;
            jumpsAvailable--;
            playerV.y = jumpSpeed;
            isPlayingJump = false;
            StartCoroutine(WaitForLand());
        }
    }

    // To seperate land and jump sound
    IEnumerator WaitForLand()
    {
      while(!controller.isGrounded)
      {
        yield return null; 
      }

      // Play sound when player is grounded
      PlayLand();
    }
    

    public void OnSprint(InputAction.CallbackContext ctxt) //sprinting
    {
        if(stamina > 0)
        {
        if(ctxt.performed)
        {
            if (!isSprinting)
            {
                isSprinting = true;
                speed *= sprintMod; 
                SubtractStamina(0.5f);
                sprintAudioSource.clip = sprintSound;
                sprintAudioSource.volume = sprintVol;
                sprintAudioSource.Play();
            }
        }
        else if(ctxt.canceled)
        {
            if (isSprinting)
            {
                isSprinting = false;
                speed /= sprintMod;
                
            }
        }
        }
        else 
        { 
            StopSprinting();

            // Checking for audio ( preventing looping on sounds )
            if(!sprintAudioSource.isPlaying)
            {
                // Playing out of Stamina if sprinting is not allowed 
                sprintAudioSource.PlayOneShot(noSprint[Random.Range(0, noSprint.Length)], noSprintVol);
                isPlayingNoSprinting = true;
            }
            
            isPlayingNoSprinting = sprintAudioSource.isPlaying;
        }
        
    }
    public void OnAbility1(InputAction.CallbackContext ctxt) //ability1
    {
        if (mage != null)
        {
            mage.OnAbility(ctxt);
        }
        else if (warrior != null)
        {
            warrior.OnAbility(ctxt);
        }
        else if (archer != null)
        {
            archer.OnAbility(ctxt);
        }
    }
    public void OnPrimaryFire(InputAction.CallbackContext ctxt) //primary attack
    {
        if (mage != null)
        {
            mage.OnPrimaryFire(ctxt);
        }
        else if (warrior != null)
        {
            warrior.OnPrimaryFire(ctxt);
        }
        else if (archer != null)
        {
            archer.OnPrimaryFire(ctxt);
        }
    }
    public void OnSecondaryFire(InputAction.CallbackContext ctxt) //secondary attack
    {
        if (mage != null)
        {
            mage.OnSecondaryFire(ctxt);
        }
        else if (warrior != null)
        {
            warrior.OnSecondaryFire(ctxt);
        }
        else if (archer != null)
        {
            archer.OnSecondaryFire(ctxt);
        }
    }

    //calculates the player movement
    void Movement()
    {
        //gets our input and adjusts the players position using a velocity formula
        movement = moveDir.x * transform.right + moveDir.z * transform.forward;
        controller.Move(speed * Time.deltaTime * movement);
        playerV.y -= gravity * Time.deltaTime;
        controller.Move(playerV * Time.deltaTime);
        //if we are on the ground play footstep sounds
        if (controller.isGrounded && movement.magnitude > 0.3 && !isPlayingSteps)
        {
            animate.SetBool("Walk", true);
            StartCoroutine(playSteps());
        }
        else if (!controller.isGrounded || movement.magnitude < 0.3)
        {
            animate.SetBool("Walk", false);
        }
        //makes sure gravity doesn't build up on a grounded player
        if (controller.isGrounded)
        {
            playerV = Vector3.zero;
            jumpsAvailable = jumpMax;
        }
    }


    //Added to fix sprinting not stopping on button up 
    private void StopSprinting()
    {
        if (isSprinting)
        {
            isSprinting = false;
            speed /= sprintMod;
        }
    }
    

    IEnumerator playSteps() //playing footsteps sounds
    {
        isPlayingSteps = true;
        audioSource.PlayOneShot(footsteps[Random.Range(0, footsteps.Length)], footstepsVol);

        // Ternary operator that checks isSprinting and returns .1f if strue and .3f if false
        yield return new WaitForSeconds(isSprinting ? 0.1f : 0.3f);
        isPlayingSteps = false;
    }

    public void Afflict(DamageStats type)
    {
        if(!isBlocking)
        {
            status = type;
            if (!isDOT)
                StartCoroutine(DamageOverTime());
        }
    }

    IEnumerator DamageOverTime()
    {
        isDOT = true;
        for (int i = 0; i < status.length; i++)
        {
            if(hp > status.damage)
            {
                hp -= status.damage;
                yield return new WaitForSeconds(1);
            }
            else if(hp <= status.damage && !isDead)
            {
                hp = 0;
                isDead = true;
                GameManager.instance.gameLost();
                isDead = false;
            }
        }
        isDOT = false;
    }

    //Timer that controls the shield regen
    IEnumerator ShieldTimer() {
        yield return new WaitForSeconds(1);
        while (timeUntilShieldRegen != 0) {
            --timeUntilShieldRegen;
            yield return new WaitForSeconds(1);
        }
    }

    //this function happens when the player is called to take damage
    public void TakeDamage(float amount)
    {
        // If the player has the shield, turn it off and start regen timer
        if (hasShield) {
            hasShield = false;
            timeUntilShieldRegen = MAX_REGEN_TIME;
            StartCoroutine(ShieldTimer());
            return;
        }

        // If they are hit before regen is over, reset timer
        if (!hasShield && timeUntilShieldRegen > 0)
            timeUntilShieldRegen = MAX_REGEN_TIME;

        //IF BLOCKING TAKE NO DAMAGE TO HEALTH, JUST LOSE STAMINA <3
        if (isBlocking)
        {
            stamina -= 1.5f;
        }
        else
            hp -= amount * damageReduction;

        if (!PhotonNetwork.InRoom && BluetoothManager.instance != null)
            BluetoothManager.instance.UpdateBarGraphHealth(hp);

        if (!isPlayingSteps) //plays hurt sounds
        {
            audioSource.PlayOneShot(hurt[Random.Range(0, hurt.Length)], hurtVol);
        }
        
        //if health drops below zero run our lose condition
        if(hp <= 0 && !isDead)
        {
            hp = 0;
            isDead = true;

            // Player death sound
            PlayPlayerDeadAud();

            //Call lose game for every player in room through RPC calls, otherwise call normally
            if (PhotonNetwork.InRoom)
                GameManager.instance.CallGameLost();
            else if (!PhotonNetwork.InRoom)
                GameManager.instance.gameLost();

            isDead = false;
        }
    }

    // The function for updating currentHP bar
    //called when player picks up a health potion
    public void AddHP(float amount)
    {
        if (hp + amount > hpBase) //added amount would exceed max HP
        { 
            hp = hpBase; //set to max currentHP
        } 
        else
        {
            hp += amount; //add amount to currentHP
        }
    }

    // Subtract & add function for currentStamina
    public void AddStamina(float amount)
    {
        if (stamina + amount > staminaBase) 
        { 
            stamina = staminaBase; 
        }
        else if(stamina + amount > 10) // Not going above ten 
        {
            stamina = 10;
        }
        else
        {
            stamina += amount; 
        }
    }

    public void SubtractStamina(float amount) 
    {
        if (stamina - amount > staminaBase) 
        { 
            stamina = staminaBase; 
        } 
        else if(stamina - amount < 0) // Not going below zero
        {
            stamina = 0;
        }
        else
        {
            stamina -= amount; 
        }
    }


    // Regenerate Stamina 
    private void RegenerateStamina()
    {
        if(stamina < staminaBase && !isRegenerating)
        {
            StartCoroutine(RegenStaminaDelay());
        }
    }

    // Preventing Stamina from regenerating too fast
    private IEnumerator RegenStaminaDelay()
    {
        // Function has been called
        isRegenerating = true; 

        // Current stamina
        float initial = stamina;
        
        // 3 second hold
        yield return new WaitForSeconds(3);

        // If current stamina after 3 secs is not lower than initial
        if(stamina >= initial && stamina < staminaBase)
        {
            // If stamina isn't full 
            while(stamina < staminaBase)
            {
                // Can adjust staminaRegenerate in Unity
               stamina += staminaRegenerate * Time.deltaTime;

                // Cap at 10, then store to make new stamina value
                if(stamina > staminaBase)
                {
                    stamina = staminaBase;
                }

               yield return null;
            }
        }
        
        // Function off
        isRegenerating = false;
    }

    
    //called when player runs into spiderwebs
    public void Slow()
    {
        speed = speed / 7;
    }

    //called when player escapes spiderwebs
    public void UnSlow()
    {
        speed = speed * 7;
    }

    //the function for updating our ui
    public void UpdatePlayerUI()
    {
        // Variable for filling health bar 
        float healthRatio = (float)hp / hpBase;
        float staminaRatio = (float)stamina / staminaBase; 

        // Smoothly lerping through values 
        HpDisplay = Mathf.Lerp(HpDisplay, healthRatio, Time.deltaTime * lerpSpeed);
        StaminaDisplay = Mathf.Lerp(StaminaDisplay, staminaRatio, Time.deltaTime * lerpSpeed);

        // Storing 
        GameManager.instance.playerHPBar.fillAmount = HpDisplay; 
        GameManager.instance.staminaBar.fillAmount = StaminaDisplay;

    
    
            // If health is more than 50% full
            if (HpDisplay > 0.5f || GameManager.instance.playerHPBar.color != midHealth) 
            {
                GameManager.instance.playerHPBar.color = Color.Lerp(midHealth, fullHealth, (HpDisplay - 0.5f) * 2);
                isPlayingNoHP = false;
            }
            else // If the health is less than 50%
            {
                GameManager.instance.playerHPBar.color = Color.Lerp(criticalHealth, midHealth, HpDisplay * 2);
            }

            // If stamina is more than 50% full 
            if (StaminaDisplay > 0.5f || GameManager.instance.staminaBar.color != midStamina) 
            {
                GameManager.instance.staminaBar.color = Color.Lerp(midStamina, fullStamina, (StaminaDisplay - 0.5f) * 2); 
            }
            else // If the stamina is less than 50%
            {
                GameManager.instance.staminaBar.color = Color.Lerp(criticalStamina, midStamina, StaminaDisplay * 2);  
            }

    }

    public void PlayJump()
    {
       // Sound for jump
        if (audioSource != null)
        {
            audioSource.PlayOneShot(playerJump, playerJumpVol);
            isPlayingJump = true;
        }
    }

    public void PlayLand()
    {
       // Sound for land
        if (audioSource != null)
        {
            audioSource.PlayOneShot(playerLand, playerLandVol);
            isPlayingLand = true;
        }
    }

    public void PlayTeleport()
    {
       // Sound for teleportation
        if (audioSource != null)
        {
            audioSource.PlayOneShot(playerTeleport, playerTeleportVol);
            isPlayingTeleport = true;
        }
    }

    public void PlayWeaponHit()
    {
       // Sound for weapon collision
        if (audioSource != null)
        {
            audioSource.PlayOneShot(WeaponHit[Random.Range(0, WeaponHit.Length)], weaponHitVol);
            isPlayingWeaponHit = true;
        }
    }

    public void PlayArrowAud()
    {
       // Sound for shooting arrow
        if (audioSource != null)
        {
            audioSource.PlayOneShot(arrowAud[Random.Range(0, arrowAud.Length)], arrowAudVol);
            isPlayingArrowAud = true;
        }
    }

    public void PlaySwordHitAud()
    {
       // Sound for sword
        if (audioSource != null)
        {
            audioSource.PlayOneShot(swordHitAud[Random.Range(0, swordHitAud.Length)], swordHitAudVol);
            isPlayingSwordHitAud = true;
        }
    }

    public void PlayArrowHitAud()
    {
       // Sound for arrow collision
        if (audioSource != null)
        {
            audioSource.PlayOneShot(arrowHitAud[Random.Range(0, arrowHitAud.Length)], arrowHitAudVol);
            isPlayingArrowHitAud = true;
        }
    }

    public void PlayPlayerDeadAud()
    {
       // Sound for player death
        if (audioSource != null)
        {
            audioSource.PlayOneShot(playerDeadAud[Random.Range(0, playerDeadAud.Length)], playerDeadAudVol);
            isPlayingPlayerDeadAud = true;
        }
    }

    public void PlayUnlockSkillAud()
    {
       // Sound for unlock skill
        if (audioSource != null)
        {
            audioSource.PlayOneShot(unlockSkillAud, unlockSkillAudVol);
            isPlayingUnlockSkillAud = true;
        }
    }

    public void PlayCantUnlockSkillAud()
    {
       // Sound for cant unlock skill
        if (audioSource != null)
        {
            audioSource.PlayOneShot(CantUnlockSkillAud, cantUnlockSkillAudVol);
            isPlayingCantUnlockSkillAud = true;
        }
    }

    public void PlayLosePointAud()
    {
       // Sound for loosing point
        if (audioSource != null)
        {
            audioSource.PlayOneShot(losePointAud, losePointAudVol);
            isPlayingLosePointAud = true;
        }
    }

    public void PlayAddPointAud()
    {
       // Sound for adding point
        if (audioSource != null)
        {
            audioSource.PlayOneShot(addPointAud, addPointAudVol);
            isPlayingAddPointAud = true;
        }
    }

    public void PlayDoorCloseAud()
    {
       // Sound for door close
        if (audioSource != null && !isPlayingDoorCloseAud)
        {
            // Random sound from array
            AudioClip clip = doorCloseAud[Random.Range(0, doorCloseAud.Length)];
            audioSource.PlayOneShot(clip, doorCloseAudVol);
            isPlayingDoorCloseAud = true;

            //Stop at clip length
            StartCoroutine(ResetCloseDoorSound(clip.length));
        }
    }

    public void PlayDoorOpenAud()
    {
       // Sound for door open
        if (audioSource != null && !isPlayingDoorOpenAud)
        {
            AudioClip clip = doorOpenAud[Random.Range(0, doorOpenAud.Length)];
            audioSource.PlayOneShot(clip, doorOpenAudVol);
            isPlayingDoorOpenAud = true;
            StartCoroutine(ResetOpenDoorSound(clip.length));
        }
    }

    public void PlayIceAud()
    {
        // Sound for ice enemy death
        if (audioSource != null)
        {
            audioSource.PlayOneShot(iceDeathAud, iceDeathAudVol);
            isPlayingIceDeathAud = true;
        }
    }
    public void PlaySkeletonAud()
    {
       // Sound for skeleton enemy death
        if (audioSource != null)
        {
            audioSource.PlayOneShot(skeletonAud[Random.Range(0, skeletonAud.Length)], skeletonAudVol);
            isPlayingskeletonAud = true;
        }
    }

    private IEnumerator ResetCloseDoorSound(float num)
    {
        yield return new WaitForSeconds(num); 
        isPlayingDoorCloseAud = false;
    }

    private IEnumerator ResetOpenDoorSound(float num)
    {
        yield return new WaitForSeconds(num); 
        isPlayingDoorOpenAud = false;
    }

    public void PlayEnemyCollsionAud()
    {
       // Sound for teleportation
        if (audioSource != null)
        {
            audioSource.PlayOneShot(collsionAud[Random.Range(0, collsionAud.Length)], collisionAudVol);
            isPlayingCollisionAud = true;
        }
    }

    public void PlayIceEnemyAud()
    {
       // Sound for teleportation
        if (audioSource != null)
        {
            audioSource.PlayOneShot(iceEnemyAud[Random.Range(0, iceEnemyAud.Length)], iceEnemyAudVol);
            isPlayingIceEnemyAud = true;
        }
    }


    
    //a simple function for respawning the player
    public void Respawn()
    {
        
        this.transform.position = GameManager.playerLocation;
        hp = hpBase;
        stamina = staminaBase;
        UpdatePlayerUI();
        GameManager.instance.ResetAllDoors();
    }
    
    //load data of a previous game
    public void LoadData(GameData data)
    {
        spawnLocation = data.playerPos;
        spawnRotation = data.playerRot;
        spawnHP = data.playerHp; 
        spawnStamina = data.playerStamina;
        SkillTreeManager.Instance.LoadData(data);
    }

    //saves all important current data
    public void SaveData(ref GameData data)
    {
        data.playerPos = transform.position;
        data.playerRot = transform.rotation;
        data.playerHp = hp;
        data.playerStamina = stamina;
        SkillTreeManager.Instance.SaveData(ref data);
    }

    //this is the new function for assign a class script to our player
    public void AssignClass(int choice)
    {
        // Prevent more than 1 class in multiplayer
        if (PhotonNetwork.InRoom)
            RemoveAllClasses();

        switch (choice)
        {
            //first we add the script, then set out script variable to the added script
            case 1:
                this.gameObject.AddComponent<Class_Warrior>();
                warrior = this.GetComponent<Class_Warrior>();
                classCase = 1;
                break;
            case 2:
                this.gameObject.AddComponent<Class_Mage>();
                mage = this.GetComponent<Class_Mage>();
                classCase = 2;
                break;
            
            case 3:
                this.gameObject.AddComponent<Class_Archer>();
                archer = this.GetComponent<Class_Archer>();
                classCase = 3;
                break;
        }
    }

    // Just clears up all the classes to be reassigned after
    void RemoveAllClasses() {
        if (warrior != null) {
            Destroy(warrior);
            warrior = null;
        }
        if (mage != null) {
            Destroy(mage);
            mage = null;
        }
        if (archer != null) {
            Destroy(archer);
            archer = null;
        }
    }

    // Makes sure your own player never loses its own class during prior steps or during disconnect
    void VerifyClassState() {
        switch (classCase)
        {
            case 1:
                warrior = this.GetComponent<Class_Warrior>();
                if (warrior == null)
                    warrior = this.gameObject.AddComponent<Class_Warrior>();
                break;
            case 2:
                mage = this.GetComponent<Class_Mage>();
                if (mage == null)
                    mage = this.gameObject.AddComponent<Class_Mage>();
                break;

            case 3:
                archer = this.GetComponent<Class_Archer>();
                if (archer == null)
                    archer = this.gameObject.AddComponent<Class_Archer>();
                break;
        }
    }

    //public functions for our class scripts to call in order to attack properly
    public void SetAnimationTrigger(string triggerName)
    {
        animate.SetTrigger(triggerName);
    }
    public void SetAnimationBool(string boolName, bool state)
    {
        animate.SetBool(boolName, state);
    }

    public void SetAnimationSpeed(float speed)
    {
        animate.speed = speed;
    }

    public void PlaySound(char context) // A for attack, 
    {
        switch (context)
        {
            case 'A':
                audioSource.PlayOneShot(attack[Random.Range(0, attack.Length)], attackVol);
                break;

        }
    }
}