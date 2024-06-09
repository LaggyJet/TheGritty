//Worked on by : Jacob Irvin, natalie lubahn

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour, IDamage
{
    //this sets up our player controller variable to handle collision
    [SerializeField] CharacterController controller;

    //these variables are modifiable in the editor
    [SerializeField] int hp;
    [SerializeField] int speed;
    [SerializeField] int sprintMod;
    [SerializeField] int gravity;

    [SerializeField] int shootDamage;
    [SerializeField] float shootRate;
    [SerializeField] int shootDistance;

    //these are variables used explicitly in functions
    int hpBase;
    bool isShooting;

    Vector3 moveDir;
    Vector3 playerV;

    // Start is called before the first frame update
    void Start()
    {
        //tracks our current hp and the hp we will update
        hpBase = hp;
        updatePlayerUI();
    }

    // Update is called once per frame
    void Update()
    {
        Movement();
        Sprint();

        if (Input.GetButton("Fire1") && !isShooting)
        {
            StartCoroutine(Shoot());
        }
    }

    void Movement()
    {
        //makes sure gravity doesn't build up on a grounded player
        if (controller.isGrounded)
        {
            playerV = Vector3.zero;
        }

        //gets our input and moves the player
        moveDir = Input.GetAxis("Horizontal") * transform.right + Input.GetAxis("Vertical") * transform.forward;

        controller.Move(moveDir * speed * Time.deltaTime);

        playerV.y -= gravity * Time.deltaTime;

        controller.Move(playerV * Time.deltaTime);
    }

    void Sprint()
    {
        if (Input.GetButtonDown("Sprint"))
        {
            speed *= sprintMod;
        }
        else if (Input.GetButtonUp("Sprint"))
        {
            speed /= sprintMod;
        }
    }

    IEnumerator Shoot()
    {
        //sets shootings variable to true so we can only fire once at a time
        isShooting = true;
        //sets up our collision detection
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

        //waits for x amount of time then sets shooting variable to false so we can fire again
        yield return new WaitForSeconds(shootRate);
        isShooting = false;
    }

    //this function happens when the player is called to take damage
    public void TakeDamage(int amount)
    {
        //subtract the damage from the player
        hp -= amount;
        updatePlayerUI();

        if(hp <= 0)
        {
            GameManager.instance.gameLost();
        }
    }

    void updatePlayerUI()
    {
        GameManager.instance.playerHPBar.fillAmount = (float)hp / hpBase;
    }
}
