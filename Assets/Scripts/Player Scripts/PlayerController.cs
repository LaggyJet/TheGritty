//Worked on by : Jacob Irvin, Natalie Lubahn, Kheera, Emily Underwood

using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour, IDamage, IDataPersistence
{
     public static PlayerController instance; 

    //this sets up our player controller variable to handle collision
    public CharacterController controller;

    //these variables are game function variables that may likely be changed
    
    [SerializeField] float speed;
    [SerializeField] int sprintMod;
    [SerializeField] int gravity;
    [SerializeField] int jumpMax;
    [SerializeField] int jumpSpeed;
    [SerializeField] public GameObject shootPosition;
    [SerializeField] public GameObject[] combatObjects;

    [Header("------- HP -------")]
    
    [Range(0f, 10f)] public float hp; 
    float hpBase;

    // Health bar gradual fill 
    [SerializeField] Color fullHealth; 
    [SerializeField] Color midHealth; 
    [SerializeField] Color criticalHealth;

    // HP bar shake
    [Range(0f, 10f)] public float hpShakeDuration;  

    [Header("------- Stamina -------")]

    [Range(0f, 10f)] public float stamina; 
    float staminaBase; 
    Coroutine staminaCor = null;
    
    
     // stamina bar gradual fill 
    [SerializeField] Color fullstamina; 
    [SerializeField] Color midstamina; 
    [SerializeField] Color criticalstamina;

    // stamina bar shake
    [Range(0f, 10f)] public float stamShakeDuration;   

    //these are animation variables
    [SerializeField] public Animator animate;
    [SerializeField] float animationTransSpeed;

    //these are variables used explicitly in functions
    DamageStats status;
    int jumpCount;
    bool isDead;
    bool isDOT;

    Vector3 moveDir;
    Vector3 playerV;

    [SerializeField] Sprite sprite;

    //variables used for save/load
    public static Vector3 spawnLocation;
    public static Quaternion spawnRotation;
    public static float spawnHp;
    public static float spawnStamina;


    [Header("------ Audio ------")]

    //Audio variables
    [SerializeField] public AudioSource audioSource;
    [SerializeField] AudioClip[] footsteps;
    [SerializeField] float footstepsVol;
    [SerializeField] AudioClip[] hurt;
    [SerializeField] float hurtVol;
    [SerializeField] public AudioClip[] attack;
    [SerializeField] float attackVol;
    [SerializeField] AudioClip[] filledStam;
    [SerializeField] float filledStamVol;
    bool isPlayingSteps;
    bool isSprinting;

    private void Start()
    {

        //tracks our base hp and the current hp that will update as our player takes damage or gets health
        hpBase = hp;
        staminaBase = stamina;
        this.transform.position = Vector3.zero;
        this.transform.rotation = Quaternion.identity;

        if (spawnLocation == Vector3.zero)
        {
            this.transform.position = GameManager.playerLocation;
            this.transform.rotation = GameManager.instance.player.transform.rotation ;
            //updates our ui to accurately show the player hp and other information
            updatePlayerUI();
        }
        else
        {
            
            GameManager.playerLocation = spawnLocation;
            this.transform.position = spawnLocation;
            this.transform.rotation = spawnRotation;
            hp = spawnHp;
            stamina = spawnStamina;
            Physics.SyncTransforms();
            //updates our ui to accurately show the player hp / stamina and other information
            updatePlayerUI();
            spawnLocation = Vector3.zero;
        }
    }

    //methods for key binding/controls
    public void OnMove(InputAction.CallbackContext ctxt)
    {
        Vector2 newMoveDir = ctxt.ReadValue<Vector2>();
        moveDir.x = newMoveDir.x;
        moveDir.z = newMoveDir.y;
       
    }
    public void OnJump(InputAction.CallbackContext ctxt)
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
    public void OnSprint(InputAction.CallbackContext ctxt)
    {
        if(ctxt.performed)
        {
            if (!isSprinting)
            {
                isSprinting = true;
                speed *= sprintMod;
                SubtractStamina(0.5f);
            }
            else if (isSprinting)
            {
                isSprinting = false;
                speed /= sprintMod;
            }
        }
    }
    public void OnPrimaryFire(InputAction.CallbackContext ctxt)
    {

        if (ctxt.performed && !isShooting && !GameManager.instance.isPaused && SceneManager.GetActiveScene().name != "title menu" && staminaCor == null)
        {
            animate.SetTrigger("PrimaryFire");
            SubtractStamina(0.5f);
        }
    }
    public void OnSecondaryFire(InputAction.CallbackContext ctxt)
    {
        if(ctxt.performed && !GameManager.instance.isPaused && SceneManager.GetActiveScene().name != "title menu")
        {
            SecondaryFireCheck();
        }
    }
    public void OnAbility1(InputAction.CallbackContext ctxt)
    {
        if (ctxt.performed)
        {
            Debug.Log("stayc girls its going down!! (testing)");
        }
    }

    // Update is called once per frame
    void Update()
    {
        //runs our movement function to determine the player velocity each frame
        Movement();
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
    IEnumerator playSteps() //playing footsteps sounds
    {
        isPlayingSteps = true;
        audioSource.PlayOneShot(footsteps[Random.Range(0, footsteps.Length)], footstepsVol);

        if (!isSprinting)
        {
            yield return new WaitForSeconds(0.3f);
        }
        else
        {
            yield return new WaitForSeconds(0.1f);
        }
        isPlayingSteps = false;
    }

    //this function handles everything to do with the player shooting
    void PrimaryFire()
    {
        //sets shootings variable to true so we can only fire once at a time
        isShooting = true;
        
        audioSource.PlayOneShot(attack[Random.Range(0, attack.Length)], attackVol);
  
        //spawns our projectile
        Instantiate(projectile, shootPosition.transform.position, shootPosition.transform.rotation);
        isShooting = false;
    }

    void SecondaryFireCheck()
    {
        if (!isShooting)
        {
            isShooting = true;
            SubtractStamina(0.5f);
            animate.SetTrigger("SecondaryFireStart");
        }
        else if (isShooting)
        {
            isShooting = false;
            animate.SetTrigger("SecondaryFireStop");
            SecondaryFireStop();
        }
    }

    void SecondaryFire()
    {
        audioSource.PlayOneShot(attack[Random.Range(0, attack.Length)], attackVol);

        fireSpray.SetActive(true);
        fireSpray.GetComponent<ParticleSystem>().Play();

    }

    private void OnParticleCollision(GameObject other)
    {
        int damage = 1;
        //when encountering a collision trigger it checks for component IDamage
        IDamage dmg = other.GetComponent<IDamage>();

        //if there is an IDamage component we run the inside code
        if (dmg != null && !other.gameObject.CompareTag("Player"))
        {
            //deal damage to the object hit
            dmg.TakeDamage(damage);
            //destroy our projectile
            Destroy(gameObject);
        }
    }


    public void Afflict(DamageStats type)
    {
        status = type;
        if (!isDOT)
            StartCoroutine(DamageOverTime());
    }

    IEnumerator DamageOverTime()
    {
        isDOT = true;
        for (int i = 0; i < status.length; i++)
        {
            TakeDamage(status.damage);
            yield return new WaitForSeconds(1);
        }
        isDOT = false;
    }

    //this function happens when the player is called to take damage
    public void TakeDamage(float amount)
    {
        //subtract the damage from the player
        hp -= amount;

        if (!isPlayingSteps) //plays hurt sounds
        {
            audioSource.PlayOneShot(hurt[Random.Range(0, hurt.Length)], hurtVol);
        }

        //updates our ui to accurately show the players health / stamina 
        updatePlayerUI();
        //if health drops below zero run our lose condition
        if(hp <= 0 && !isDead)
        {
            isDead = true;
            GameManager.instance.gameLost();
            isDead = false;
        }
    }

    // The function for updating HP bar
    //called when player picks up a health potion
    public void AddHP(float amount)
    {
        if (hp + amount > hpBase) { //added amount would exceed max hp
            hp = hpBase; //set to max hp
        } else
        {
            hp += amount; //add amount to hp
        }
        updatePlayerUI();
    }

    // Subtract & add function for stamina
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
        updatePlayerUI();
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

        updatePlayerUI();
    }

    IEnumerator StaminaDecreaseRoutine()
    {
        while(true) 
        {
            SubtractStamina(0.1f);

           yield return new WaitForSeconds(0.5f);
        }
       
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
    public void updatePlayerUI()
    {
        // Variable for filling bar 
        float healthRatio = (float)hp / hpBase;
        float staminaRatio = (float)stamina / staminaBase; 

        // Storing 
        GameManager.instance.playerHPBar.fillAmount = healthRatio; 
        GameManager.instance.staminaBar.fillAmount = staminaRatio;

      
       
            // If health is more than 50% full
            if (healthRatio > 0.5f || GameManager.instance.playerHPBar.color != midHealth) 
            {
                GameManager.instance.playerHPBar.color = Color.Lerp(midHealth, fullHealth, (healthRatio - 0.5f) * 2);
            }
            else // If the health is less than 50%
            {
                GameManager.instance.playerHPBar.color = Color.Lerp(criticalHealth, midHealth, healthRatio * 2);
                if(healthRatio <= 0.5f ){
                   Shake.instance.Shaking(hpShakeDuration); 
                }
                
            }
       
       
       
            // If stamina is more than 50% full 
            if (staminaRatio > 0.5f || GameManager.instance.staminaBar.color != midstamina) 
            {
                GameManager.instance.staminaBar.color = Color.Lerp(midstamina, fullstamina, (staminaRatio - 0.5f) * 2); 
            }
            else // If the stamina is less than 50%
            {
                GameManager.instance.staminaBar.color = Color.Lerp(criticalstamina, midstamina, staminaRatio * 2);
                if(staminaRatio <= 0.5f){
                   Shake.instance.Shaking(stamShakeDuration); 
                }
                
            }
       
    }
    
    public void Respawn()
    {
        this.transform.position = GameManager.playerLocation;
        hp = hpBase;
        stamina = staminaBase;
        updatePlayerUI(); 
    }
    public void LoadData(GameData data)
    {
        spawnLocation = data.playerPos;
        spawnRotation = data.playerRot;
        spawnHp = data.playerHp; 
        spawnStamina = data.playerStamina;
    }
    public void SaveData(ref GameData data)
    {
        data.playerPos = this.transform.position;
        data.playerRot = this.transform.rotation;
        data.playerHp = hp;
        data.playerStamina = stamina;
    }
}
