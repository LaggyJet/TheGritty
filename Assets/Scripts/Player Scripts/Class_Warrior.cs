//Worked on By : Jacob Irvin, Joshua Furber
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Class_Warrior : MonoBehaviour
{
    PlayerController player;
    bool isAttacking;
    float damage = 10;
    bool canDOT = true;

    private void Start()
    {
        player = GameManager.instance.player.GetComponent<PlayerController>();
        player.combatObjects[3].AddComponent<MeleeWeaponController>().SetWeapon(damage, canDOT, type);
        player.combatObjects[3].SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        if (!GameManager.instance.isPaused && SceneManager.GetActiveScene().name != "title menu")
        {
            AttackCheck();
        }
    }

    void AttackCheck()
    {
        if (Input.GetButtonDown("Fire1"))
        {
            //plays our primary shooting animation
            player.SetAnimationTrigger("Warrior1");
        }
        if (Input.GetButtonDown("Fire2") && !isAttacking)
        {
            isAttacking = true;
            player.SetAnimationBool("Warrior2", true);
        }
        if (Input.GetButtonUp("Fire2") && isAttacking)
        {
            isAttacking = false;
            player.SetAnimationBool("Warrior2", false);
        }
    }
    void WeaponColliderOn() 
    {
        player.combatObjects[3].GetComponent<Collider>().enabled = true; 
    }
    void WeaponColliderOff() 
    {
        player.combatObjects[3].GetComponent<Collider>().enabled = false; 
    }

    public void SwingStart()
    {
        isAttacking = true;
        WeaponColliderOn();
    }
    public void SwingEnd()
    {
        WeaponColliderOff();
        isAttacking = false;
    }


}
