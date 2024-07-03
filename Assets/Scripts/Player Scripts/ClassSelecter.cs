using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClassSelecter : MonoBehaviour
{
    GameObject player;

    public void AssignClass(int classNumber) // Class 0 is wretch(base class) class 1 is Mage, class 2 is Warrior, class 3 is archer
    {
        switch(classNumber)
        {
            case 0:
                player.AddComponent<Class_Wretch>();
                break;
            case 1:
                player.AddComponent<Class_Mage>();
                break;
            case 2:
                player.AddComponent<Class_Warrior>();
                break;
            case 3:
                player.AddComponent<Class_Archer>();
                break;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        player = GameManager.instance.player;
        AssignClass(1);
    }
}
