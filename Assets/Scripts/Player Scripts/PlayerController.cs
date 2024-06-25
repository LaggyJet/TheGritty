//Worked on by : Jacob Irvin, Natalie Lubahn, Kheera

using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class PlayerController : MonoBehaviour, IDamage, IDataPersistence
{
    //this sets up our player controller variable to handle collision
    public CharacterController controller;

    //these variables are game function variables that may likely be changed
    [SerializeField] bool shootProjectile;
    [SerializeField] float hp;
    [SerializeField] int speed;
    [SerializeField] int sprintMod;
    [SerializeField] int gravity;
    [SerializeField] int jumpMax;
    [SerializeField] int jumpSpeed;

    // Health bar gradual fill 
    [SerializeField] Color fullHealth; 
    [SerializeField] Color midHealth; 
    [SerializeField] Color criticalHealth;

    // HP bar shake
    [Range(0f, 10f)] public float duration;  
   

    //these are combat variables
    [SerializeField] float shootDamage;
    [SerializeField] float shootRate;
    [SerializeField] int shootDistance;
    [SerializeField] GameObject shootPosition;
    [SerializeField] GameObject projectile;

    //these are animation variables
    [SerializeField] Animator animate;
    [SerializeField] float animationTransSpeed;


    //these are variables used explicitly in functions
    DamageStats status;
    int jumpCount;
    float hpBase;
    bool isShooting;
    bool isDead;
    bool isDOT;

    Vector3 moveDir;
    Vector3 playerV;

    //variables used for save/load
    public static Vector3 spawnLocation;
    public static Quaternion spawnRotation;
    public static float spawnHp;

    private void Awake()
    {
        //tracks our base hp and the current hp that will update as our player takes damage or gets health
        hpBase = hp;
        if (spawnLocation == Vector3.zero)
        {
            this.transform.position = new Vector3(4.1992116f, 0.0799998641f, 49.6620026f);
            this.transform.rotation = new Quaternion(0, 180.513367f, 0, 0);
            //updates our ui to accurately show the player hp and other information
            updatePlayerUI();
        }
        else
        {
            this.transform.position = spawnLocation;
            this.transform.rotation = spawnRotation;
            hp = spawnHp;
            //updates our ui to accurately show the player hp and other information
            updatePlayerUI();
        }
    }

    // Update is called once per frame
    void Update()
    {
        //runs our movement function to determine the player velocity each frame
        Movement();
        Sprint();

        if (Input.GetButton("Fire1") && !isShooting && !GameManager.instance.isPaused)
        {
            //starts our shooting function
            StartCoroutine(Shoot());
        }

        else if (Input.GetButton("Ability1") && !isShooting && !GameManager.instance.isPaused)
        {
            //starts our shooting function
            StartCoroutine(AbilityOne());
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
    }

    //calculates our speed if the player is sprinting
    void Sprint()
    {
        //when Sprint is pressed apply the sprint modifier variable to our speed variable
        if (Input.GetButtonDown("Sprint"))
        {
            speed *= sprintMod;
        }
        //when sprint is no longer being pressed we remove the sprint modifier from the speed variable
        else if (Input.GetButtonUp("Sprint"))
        {
            speed /= sprintMod;
        }
    }

    //this function handles everything to do with the player shooting
    IEnumerator Shoot()
    {
        //sets shootings variable to true so we can only fire once at a time
        isShooting = true;

        //plays our shooting animation
        animate.SetTrigger("Shoot Fire");
        //sets up our collision detection
        if(!shootProjectile)
        {
            RaycastHit hit;
            if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, shootDistance))
            {
                Debug.Log(hit.transform.name);

                IDamage dmg = hit.collider.GetComponent<IDamage>();

                if (hit.transform != transform && dmg != null)
                {
                    dmg.TakeDamage(shootDamage);
                }
            }
        }
        //spawns our projectile
        else
        {
            yield return new WaitForSeconds(.2f);
            Instantiate(projectile, shootPosition.transform.position, shootPosition.transform.rotation);
        }
        

        //waits for x amount of time then sets shooting variable to false so we can fire again
        yield return new WaitForSeconds(shootRate);
        isShooting = false;
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
        //updates our ui to accurately show the players health
        updatePlayerUI();
        //if health drops below zero run our lose condition
        if(hp <= 0 && !isDead)
        {
            isDead = true;
            GameManager.instance.gameLost();
        }
    }

    //the function for updating our ui
    void updatePlayerUI()
    {
        // Variable for filling bar 
        float healthRatio = (float)hp / hpBase;

        // Storing 
        GameManager.instance.playerHPBar.fillAmount = healthRatio;

        if (healthRatio > 0.5f || GameManager.instance.playerHPBar.color != midHealth) // If health is more than 50% full
        {
            GameManager.instance.playerHPBar.color = Color.Lerp(midHealth, fullHealth, (healthRatio - 0.5f) * 2);
        }
        else // If the health is less than 50%
        {
            GameManager.instance.playerHPBar.color = Color.Lerp(criticalHealth, midHealth, healthRatio * 2); 
            Shake.instance.Shaking(duration);  
        }
    }
    
    public void Respawn()
    {
        this.transform.position = GameManager.instance.playerLocation;
        hp = hpBase;
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
}
