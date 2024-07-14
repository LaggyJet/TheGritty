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

public class PlayerController : MonoBehaviourPunCallbacks, IDamage, IDataPersistence
{
    //lets us access player and controller and character controller from outside the object
    public static PlayerController instance; 
    public CharacterController controller;

    [SerializeField] Animator animate;

    [Header("------- Movement and Position -------")]
    [SerializeField] float speed;
    [SerializeField] int sprintMod;
    [SerializeField] int gravity;
    [SerializeField] int jumpMax;
    [SerializeField] int jumpSpeed;
    int jumpsAvailable;
    Vector3 moveDir;
    public Vector3 playerV;
    Vector3 networkPos;
    Quaternion networkRot;
    public static Vector3 spawnLocation;
    public static Quaternion spawnRotation;


    [Header("------- HP -------")]
    DamageStats status;
    bool isDead;
    bool isDOT;
    public static float spawnHP;
    // Health bar colors and script for shaking the ui
    [Range(0f, 10f)] public float hp; 
    public float hpBase; 

    // Health bar gradual fill 
    [SerializeField] Color fullHealth; 
    [SerializeField] Color midHealth; 
    [SerializeField] Color criticalHealth;
    [SerializeField] public Shake hpShake;


    [Header("------- Stamina -------")]
    [Range(0f, 10f)] public float stamina;
    [Range(0f, 50f)] public float staminaRegenerate;
    public static float spawnStamina;
    bool isRegenerating;

    [SerializeField] Color fullStamina; 
    [SerializeField] Color midStamina; 
    [SerializeField] Color criticalStamina;
    [SerializeField] public Shake stamShake; 
    public float staminaBase;

    [Header("------- Combat -------")]
    [SerializeField] public GameObject shootPosition;
    [SerializeField] public GameObject arrowPosition;
    [SerializeField] public GameObject[] combatObjects;
    ClassSelection classSelector;
    public bool useStamina = true;
    public bool isBlocking = false;
    public int classCase;
    public Class_Mage mage = null;
    public Class_Warrior warrior = null;
    public Class_Archer archer = null;


    [Header("------ Audio ------")]
    [SerializeField] public AudioSource audioSource;
    [SerializeField] AudioClip[] footsteps;
    [SerializeField] float footstepsVol;
    [SerializeField] AudioClip[] hurt;
    [SerializeField] float hurtVol;
    [SerializeField] public AudioClip[] attack;
    [SerializeField] float attackVol;
    bool isPlayingSteps;


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

    private const string CLASS_SELECTED = "ClassSelected";
    ClassSelection currentClassSelection;

    private void Start() {
        // Get base selection (if possible)
        currentClassSelection = GameObject.FindWithTag("ClassSelector")?.GetComponent<ClassSelection>();

        // Setting up class for yourself or multiplayer
        if (PhotonNetwork.InRoom || !PhotonNetwork.IsConnected) {
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
            Destroy(GetComponentInChildren<Camera>().gameObject);
            Destroy(GetComponentInChildren<AudioListener>());
            UpdateOtherPlayer(gameObject);
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
        //if these are currently null we search for them every frame until we find them
        if (stamShake == null || hpShake == null)
        {
            stamShake = GameManager.instance.staminaBar.GetComponent<Shake>();
            hpShake = GameManager.instance.playerHPBar.GetComponent<Shake>();
        }

        // Prevent movement of other players
        if (!PhotonNetwork.IsConnected || GetComponent<PhotonView>().IsMine)
        {
            //runs our movement function to determine the player velocity each frame
            Movement();
            // Regenerating over time ( can be adjusted in unity )
            RegenerateStamina();
            //updates our ui every frame
            UpdatePlayerUI();
        }
    }

    void UpdateOtherPlayer(GameObject obj) {
        if (!obj) return;

        obj.layer = 0;
        foreach (Transform child in obj.transform) {
            if (!child) return;
            UpdateOtherPlayer(child.gameObject);
        }
    }

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
            //subtracts our stamina and jumps then adds our y velocity to player
            stamina -= .5f;
            jumpsAvailable--;
            playerV.y = jumpSpeed;
        }
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
        Vector3 movement = moveDir.x * transform.right + moveDir.z * transform.forward;
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

    //this function happens when the player is called to take damage
    public void TakeDamage(float amount)
    {
        //IF BLOCKING TAKE NO DAMAGE TO HEALTH, JUST LOSE STAMINA <3
        if(isBlocking)
        {
            stamina -= 1.5f;
        }
        else
            hp -= amount;

        if (!PhotonNetwork.IsConnected && BluetoothManager.instance != null)
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

            //Call lose game for every player in room through RPC calls, otherwise call normally
            if (PhotonNetwork.InRoom)
                GameManager.instance.CallGameLost();
            else if (!PhotonNetwork.IsConnected)
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
        isRegenerating = true; 

        yield return new WaitForSeconds(5);

        if(stamina < staminaBase)
        {
            stamina += staminaRegenerate * Time.deltaTime;

            if(stamina > staminaBase)
            {
                stamina = staminaBase;
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
        // Variable for filling health bar 
        float healthRatio = (float)hp / hpBase;
        float staminaRatio = (float)stamina / staminaBase; 

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

                if(healthRatio <= 0.5f )
                {
                    hpShake.Shaking(); 
                }

                if(!isPlayingNoHP)
                {
                    if(!staminaAudioSource.isPlaying)
                    {
                        // Playing heart beat for low HP 
                        staminaAudioSource.PlayOneShot(noHP[Random.Range(0, noHP.Length)], noHPvol);
                        isPlayingNoHP = true;
                        isPlayingNoHP = staminaAudioSource.isPlaying;
                    }
                }

                isPlayingNoHP = staminaAudioSource.isPlaying;
            
                if(healthRatio <= 0.5f )
                {
                    hpShake.Shaking();   
                }
                
            }

            // If stamina is more than 50% full 
            if (staminaRatio > 0.5f || GameManager.instance.staminaBar.color != midStamina) 
            {
                GameManager.instance.staminaBar.color = Color.Lerp(midStamina, fullStamina, (staminaRatio - 0.5f) * 2);
                Color color = GameManager.instance.staminaBar.color;
                color.a = Mathf.Clamp(255, 0, 1);
                GameManager.instance.staminaBar.color = color;
        }
            else if (staminaRatio <= 0.5f) // If the stamina is less than 50%
            {
                GameManager.instance.staminaBar.color = Color.Lerp(criticalStamina, midStamina, staminaRatio * 2);
                stamShake.Shaking();
            }

    }
    
    //a simple function for respawning the player
    public void Respawn()
    {
        this.transform.position = GameManager.playerLocation;
        hp = hpBase;
        stamina = staminaBase;
        UpdatePlayerUI(); 
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
        data.playerHp = hp;
        data.playerStamina = stamina;
    }

    //this is the new function for assign a class script to our player
    public void AssignClass(int choice)
    {
        RemoveAllClasses();

        switch (choice)
        {
            //first we add the script, then set out script variable to the added script
            case 1:
                this.AddComponent<Class_Warrior>();
                warrior = this.GetComponent<Class_Warrior>();
                classCase = 1;
                break;
            case 2:
                this.AddComponent<Class_Mage>();
                mage = this.GetComponent<Class_Mage>();
                classCase = 2;
                break;
            
            case 3:
                this.AddComponent<Class_Archer>();
                archer = this.GetComponent<Class_Archer>();
                classCase = 3;
                break;
        }
    }

    void RemoveAllClasses() {
        if (warrior != null && classCase != 1) {
            Destroy(warrior);
            warrior = null;
        }
        if (mage != null && classCase != 2) {
            Destroy(mage);
            mage = null;
        }
        if (archer != null && classCase != 3) {
            Destroy(archer);
            archer = null;
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