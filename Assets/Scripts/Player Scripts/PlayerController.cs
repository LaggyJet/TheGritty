//Worked on by : Jacob Irvin, Natalie Lubahn, Kheera, Emily Underwood, Joshua Furber

using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.InputSystem;


//OKAY GANG IM GONNA COMMENT ALL THIS TO THE BEST OF MY ABILITY BECAUSE THIS CODE IS STARTING TO LOOK LIKE A SHITTY FANFIC
//BUT UNLIKE SHITTY FANFIC I DONT HAVE A 2 HOUR YOUTUBE DOC TO EXPLAIN IT TO ME ~ Jacob <3
public class PlayerController : MonoBehaviourPun, IDamage, IDataPersistence
{
    //gets our player controller for movement and character controller for collision
    public static PlayerController instance; 
    public CharacterController controller;


    //these variables are game function variables that may likely be changed
    [SerializeField] float speed;
    [SerializeField] int sprintMod;
    [SerializeField] int gravity;
    [SerializeField] int jumpMax;
    [SerializeField] int jumpSpeed;
    [SerializeField] public GameObject shootPosition;
    [SerializeField] public GameObject[] combatObjects;
    public bool useStamina = true;
    public bool isBlocking = false;

    //shows up as a divider in the script
    [Header("------- HP -------")]
    
    //our player currentHP and the version saved for our max currentHP
    [Range(0f, 10f)] public float currentHP; 
    float maxHP;

    // Health bar gradual fill 
    [SerializeField] Color fullHealth; 
    [SerializeField] Color midHealth; 
    [SerializeField] Color criticalHealth;

    // currentHP bar shake duration
    [Range(0f, 10f)] public float HPShakeDuration;

    //shows up as a divider in the script
    [Header("------- Stamina -------")]

    [Range(0f, 10f)] public float currentStamina; 
    [Range(0f, 50f)] public float staminaRegenerate;  
    float maxStamina; 
    
     // currentStamina bar gradual fill 
    [SerializeField] Color fullStamina; 
    [SerializeField] Color midStamina; 
    [SerializeField] Color criticalStamina;

    // currentStamina bar shake duration
    [Range(0f, 10f)] public float staminaShakeDuration;   

    //these are animation variables
    [SerializeField] Animator animate;
    [SerializeField] float animationTransSpeed;

    //!guys what is this
    [SerializeField] Sprite sprite;

    //variables used for save/load
    public static Vector3 spawnLocation;
    public static Quaternion spawnRotation;
    public static float spawnHP;
    public static float spawnStamina;

    //shows up as a divider in the script
    [Header("------ Audio ------")]

    //Audio variables
    [SerializeField] public AudioSource audioSource;
    [SerializeField] AudioClip[] footsteps;
    [SerializeField] float footstepsVol;
    [SerializeField] AudioClip[] hurt;
    [SerializeField] float hurtVol;
    [SerializeField] public AudioClip[] attack;
    [SerializeField] float attackVol;

    [Header("------ Sprint Audio ------")]
    [SerializeField] public AudioSource sprintAudioSource;
    [SerializeField] public AudioClip sprintSound;
    [SerializeField] float sprintVol;
    [SerializeField] public AudioClip[] noSprint;
    [SerializeField] float noSprintVol; 

    [Header("------ currentStamina currentHP Audio ------")]
    [SerializeField] public AudioSource staminaAudioSource;
    [SerializeField] public AudioClip[] noHP;
    [SerializeField] public float noHPvol;
    [SerializeField] public AudioClip[] noAttack;
    [SerializeField] public float noAttackVol;



    //shows up as a divider in the script
    [Header("------ Classes ------")]
    //class variables
    [SerializeField] private ClassSelection playerClass;
    public Class_Mage mage = null;
    public Class_Warrior warrior = null;
    public Class_Archer archer = null;

    //these are variables used explicitly in functions
    DamageStats status;
    int jumpCount;
    bool isDead;
    bool isDOT;
    Vector3 moveDir;
    Vector3 playerV;
    Vector3 networkPos;
    Quaternion networkRot;
    bool isPlayingSteps;
    bool isSprinting;
    public bool isPlayingStamina;
    public bool isPlayingNoSprinting;
    public bool isPlayingNoHP = false;
    private bool isRegenerating = false;


    private void Start()
    {
        //assigns our player class
        AssignClass(playerClass.MyClass);

        // Prevent movement of other players
        //! What are the checks, why are we preventing movement, why are there destroys called? ~Jacob
        if (!GetComponent<PhotonView>().IsMine && PhotonNetwork.InRoom) {
            Destroy(GetComponentInChildren<Camera>().gameObject);
            Destroy(GetComponentInChildren<AudioListener>());
            Destroy(this);
            return;
        }

        //Singleton code
        //!whoever wrote this, if it is singleton just erase this line, if not pls comment
        if (instance == null) instance = this;
        else { Destroy(gameObject); return; }

        //tracks our base currentHP and the current currentHP that will update as our player takes damage or gets health
        maxHP = currentHP;
        maxStamina = currentStamina;
        this.transform.position = Vector3.zero;
        this.transform.rotation = Quaternion.identity;

        //calls a function to set the player variable in the game manager
       GameManager.instance.SetPlayer();

        //this is our spawn function
        if (spawnLocation == Vector3.zero) //checks if the spawnLocation is a vector 3 zero, meaning it is a new game
        {
            transform.position = GameManager.playerLocation;
            transform.rotation = GameManager.instance.player.transform.rotation ;
        }
        else //otherwise the spawnLocation has a value and means the game is being resumed
        {
            GameManager.playerLocation = spawnLocation;
            transform.position = spawnLocation;
            transform.rotation = spawnRotation;
            currentHP = spawnHP;
            currentStamina = spawnStamina;
            Physics.SyncTransforms();
            spawnLocation = Vector3.zero;
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Prevent movement of other players
        if (!PhotonNetwork.IsConnected || GetComponent<PhotonView>().IsMine)
        {
            //runs our movement function to determine the player velocity each frame
            Movement();
            // Regenerating over time ( can be adjusted in unity )
            RegenerateStamina();
            UpdatePlayerUI();
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
        if (ctxt.performed && GameManager.instance.canJump)
        {
            if (jumpCount < jumpMax)
            {
                jumpCount++;
                playerV.y = jumpSpeed;
            }
        }
        controller.Move(moveDir * speed * Time.deltaTime);
        playerV.y -= gravity * Time.deltaTime;
        controller.Move(playerV * Time.deltaTime);
    }

    public void OnSprint(InputAction.CallbackContext ctxt) //sprinting
    {
        if(ctxt.performed && currentStamina >= 0.5f)
        {
            if (!isSprinting)
            {
                isSprinting = true;
                speed *= sprintMod; 
                currentStamina -= 0.5f;
                sprintAudioSource.clip = sprintSound;
                sprintAudioSource.volume = sprintVol;
                sprintAudioSource.Play();
            }
        }
        else if(ctxt.canceled || currentStamina <= 0.5f)
        {
            if (isSprinting)
            {
                isSprinting = false;
                speed /= sprintMod;
                
            }
        }
        else 
        { 
            StopSprinting();

            // Checking for audio ( preventing looping on sounds )
            if(!sprintAudioSource.isPlaying)
            {
                // Playing out of currentStamina if sprinting is not allowed 
                sprintAudioSource.PlayOneShot(noSprint[Random.Range(0, noSprint.Length)], noSprintVol);
                isPlayingNoSprinting = true;
            }
            
            isPlayingNoSprinting = sprintAudioSource.isPlaying;
            Debug.Log("No currentStamina poo :(");    
        }
        
    }
    public void OnAbility1(InputAction.CallbackContext ctxt) //ability1
    {
        if (mage != null)
        {
            mage.OnAbility(ctxt);
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
        //makes sure gravity doesn't build up on a grounded player
        if (controller.isGrounded)
        {
            playerV = Vector3.zero;
            jumpCount = 0;
        }
        //gets our input and adjusts the players position using a velocity formula
        Vector3 movement = moveDir.x * transform.right + moveDir.z * transform.forward;
        controller.Move(movement * speed * Time.deltaTime);
        playerV.y -= gravity * Time.deltaTime;
        controller.Move(playerV * Time.deltaTime);

        if (controller.isGrounded && movement.magnitude > 0.3 && !isPlayingSteps)
        {
            StartCoroutine(playSteps());
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
            if(currentHP > status.damage)
            {
                currentHP -= status.damage;
                yield return new WaitForSeconds(1);
            }
            else if(currentHP <= status.damage && !isDead)
            {
                currentHP = 0;
                isDead = true;
                GameManager.instance.gameLost();
                isDead = false;
            }
        }
        isDOT = false;
    }

    //this function happens when the player is called to take damage
    public void TakeDamage(float amount)
    {
        if(isBlocking)
        {
            currentStamina -= 1;
        }
        else
            currentHP -= amount; 

        if (!isPlayingSteps) //plays hurt sounds
        {
            audioSource.PlayOneShot(hurt[Random.Range(0, hurt.Length)], hurtVol);
        }
        //if health drops below zero run our lose condition
        if(currentHP <= 0 && !isDead)
        {
            currentHP = 0;
            isDead = true;
            GameManager.instance.gameLost();
            isDead = false;
        }
    }

    // The function for updating currentHP bar
    //called when player picks up a health potion
    public void AddHP(float amount)
    {
        if (currentHP + amount > maxHP) { //added amount would exceed max currentHP
            currentHP = maxHP; //set to max currentHP
        } else
        {
            currentHP += amount; //add amount to currentHP
        }
    }

    // Subtract & add function for currentStamina
    public void AddStamina(float amount)
    {
        if (currentStamina + amount > maxStamina) 
        { 
            currentStamina = maxStamina; 
        }
        else if(currentStamina + amount > 10) // Not going above ten 
        {
            currentStamina = 10;
        }
        else
        {
            currentStamina += amount; 
        }
    }


    // Regenerate currentStamina 
    private void RegenerateStamina()
    {
       if(currentStamina < maxStamina && !isRegenerating)
       {
           StartCoroutine(RegencurrentStaminaDelay());
       }
    }

    // Preventing currentStamina from regenerating too fast
    private IEnumerator RegencurrentStaminaDelay()
    {
       isRegenerating = true; 

       yield return new WaitForSeconds(5);

       if(currentStamina < maxStamina)
       {
          currentStamina += staminaRegenerate * Time.deltaTime;

          if(currentStamina > maxStamina)
          {
            currentStamina = maxStamina;
          }

          yield return null;
       }

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
        // Variable for filling the health and stamina bar based on the ratio of current and max
        float healthRatio = (float)currentHP / maxHP;
        float staminaRatio = (float)currentStamina / maxStamina; 

        // Storing 
        GameManager.instance.playerHPBar.fillAmount = healthRatio; 
        GameManager.instance.staminaBar.fillAmount = staminaRatio;

      
       
            // If health is more than 50% full
            if (healthRatio > 0.5f || GameManager.instance.playerHPBar.color != midHealth) 
            {
                GameManager.instance.playerHPBar.color = Color.Lerp(midHealth, fullHealth, (healthRatio - 0.5f) * 2);
                isPlayingNoHP = false;
            }
            else // If the health is less than 50%
            {
                GameManager.instance.playerHPBar.color = Color.Lerp(criticalHealth, midHealth, healthRatio * 2);

                if(!isPlayingNoHP)
                {
                    if(!staminaAudioSource.isPlaying)
                    {
                        // Playing heart beat for low currentHP 
                        staminaAudioSource.PlayOneShot(noHP[Random.Range(0, noHP.Length)], noHPvol);
                        isPlayingNoHP = true;
                    }
                }

                isPlayingNoHP = staminaAudioSource.isPlaying;
                Debug.Log("No currentHP :(");
               
                // if(healthRatio <= 0.5f )
                // {
                //    Shake.instance.Shaking(HPShakeDuration); 
                // }
                
            }
       
            // If stamina is more than 50% full 
            if (staminaRatio > 0.5f || GameManager.instance.staminaBar.color != midStamina) 
            {
                GameManager.instance.staminaBar.color = Color.Lerp(midStamina, fullStamina, (staminaRatio - 0.5f) * 2); 
            }
            else // If the stamina is less than 50%
            {
                GameManager.instance.staminaBar.color = Color.Lerp(criticalStamina, midStamina, staminaRatio * 2);
                if(staminaRatio <= 0.5f){
                   Shake.instance.Shaking(staminaShakeDuration); 
                }
                
            }
       
    }
    
    //a simple function for respawning the player
    public void Respawn()
    {
        this.transform.position = GameManager.playerLocation;
        currentHP = maxHP;
        currentStamina = maxStamina;
    }
    
    //load data of a previous game
    public void LoadData(GameData data)
    {
        spawnLocation = data.playerPos;
        spawnRotation = data.playerRot;
        spawnHP = data.playerHp; 
        spawnStamina = data.playerStamina;
    }

    //saves all important current data
    public void SaveData(ref GameData data)
    {
        data.playerPos = transform.position;
        data.playerRot = transform.rotation;
        data.playerHp = currentHP;
        data.playerStamina = currentStamina;
    }

    //this is the new function for assign a class script to our player
    public void AssignClass(int choice)
    {
        switch (choice)
        {
            //first we add the script, then set out script variable to the added script
            case 1:
                this.AddComponent<Class_Warrior>();
                warrior = this.GetComponent<Class_Warrior>();
                break;
            case 2:
                this.AddComponent<Class_Mage>();
                mage = this.GetComponent<Class_Mage>();
                break;
            
            case 3:
                this.AddComponent<Class_Archer>();
                archer = this.GetComponent<Class_Archer>();
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