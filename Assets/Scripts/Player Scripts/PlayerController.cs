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

public class PlayerController : MonoBehaviourPun, IDamage, IDataPersistence, IPunObservable
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

    Vector3 networkPos;
    Quaternion networkRot;

    [SerializeField] Sprite sprite;

    //variables used for save/load
    public static Vector3 spawnLocation;
    public static Quaternion spawnRotation;
    public static float spawnHp;


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
        // Prevent movement of other players
        if (!GetComponent<PhotonView>().IsMine) {
            Destroy(GetComponentInChildren<Camera>().gameObject);
            Destroy(GetComponentInChildren<AudioListener>());
            Destroy(this);
            return;
        }

        if (instance == null) instance = this;
        else { Destroy(gameObject); return; }

        //tracks our base hp and the current hp that will update as our player takes damage or gets health
        hpBase = hp;
        staminaBase = stamina;
        this.transform.position = Vector3.zero;
        this.transform.rotation = Quaternion.identity;

        GameManager.instance.SetPlayer();

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
            Physics.SyncTransforms();
            //updates our ui to accurately show the player hp / stamina and other information
            updatePlayerUI();
            spawnLocation = Vector3.zero;
        }
    }

    void FixedUpdate()
    {
        //hp = hp+1;
    }

    // Update is called once per frame
    void Update()
    {
        // Prevent movement of other players
        if (GetComponent<PhotonView>().IsMine) {
            //runs our movement function to determine the player velocity each frame
            Movement();
            Sprint();
        }
        else if (!GetComponent<PhotonView>().IsMine) {
            transform.position = Vector3.Lerp(transform.position, networkPos, Time.deltaTime * 10);
            transform.rotation = Quaternion.Lerp(transform.rotation, networkRot, Time.deltaTime * 10);
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
        //runs a check for if player jumps
        if (Input.GetButtonDown("Jump") && GameManager.instance.canJump)
        {
            if (jumpCount < jumpMax)
            {
                jumpCount++;
                playerV.y = jumpSpeed;
            }
        }
        //gets our input and adjusts the players position using a velocity formula
        moveDir = Input.GetAxis("Horizontal") * transform.right + Input.GetAxis("Vertical") * transform.forward;
        controller.Move(moveDir * speed * Time.deltaTime);
        playerV.y -= gravity * Time.deltaTime;
        controller.Move(playerV * Time.deltaTime);

        if (controller.isGrounded && moveDir.magnitude > 0.3 && !isPlayingSteps)
        {
            StartCoroutine(playSteps());
        }
    }

    //calculates our speed if the player is sprinting
    void Sprint()
    {
        //when Sprint is pressed apply the sprint modifier variable to our speed variable
        if (Input.GetButtonDown("Sprint"))
        {
            isSprinting = true;
            speed *= sprintMod;
            SubtractStamina(0.5f);
        }
        //when sprint is no longer being pressed we remove the sprint modifier from the speed variable
        else if (Input.GetButtonUp("Sprint"))
        {
            isSprinting = false;
            speed /= sprintMod;
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

        // Ternary operator that checks isSprinting and returns .1f if strue and .3f if false
        yield return new WaitForSeconds(isSprinting ? 0.1f : 0.3f);
        isPlayingSteps = false;
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
    }
    public void SaveData(ref GameData data)
    {
        data.playerPos = this.transform.position;
        data.playerRot = this.transform.rotation;
        data.playerHp = hp;
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
        if (stream.IsWriting) {
            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation);
        }
        else {
            networkPos = (Vector3)stream.ReceiveNext();
            networkRot = (Quaternion)stream.ReceiveNext();
        }
    }
}
