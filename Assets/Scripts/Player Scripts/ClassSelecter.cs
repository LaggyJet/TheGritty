using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class ClassSelecter : MonoBehaviour
{
    GameObject player;
    PlayerController controller;
    public PlayerInput input;
    private void Start()
    {
        player = GameManager.instance.player;
        controller = GameManager.instance.playerScript;
    }

    public void AssignClass(int classNumber) // Class 0 is wretch(base class) class 1 is Mage, class 2 is Warrior, class 3 is archer
    {
        switch(classNumber)
        {
            case 0:
                player.AddComponent<Class_Wretch>();
                break;
            case 1:
                //player.AddComponent<Class_Mage>();
                GameManager.instance.playerScript.mage.enabled = true;
                GameManager.instance.playerScript.warrior.enabled = false;
                GameManager.instance.playerScript.archer.enabled = false;
                break;
            case 2:
                //player.AddComponent<Class_Warrior>();
                GameManager.instance.playerScript.mage.enabled = false;
                GameManager.instance.playerScript.warrior.enabled = true;
                GameManager.instance.playerScript.archer.enabled = false;
                break;
            case 3:
                //player.AddComponent<Class_Archer>();
                GameManager.instance.playerScript.mage.enabled = false;
                GameManager.instance.playerScript.warrior.enabled = false;
                GameManager.instance.playerScript.archer.enabled = true;
                break;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        AssignClass(1);
    }
}
