//Made by Emily Underwood
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class DoorLower : MonoBehaviour
{
    public GameObject enemy; //enemy that needs killed to open door


    public void lowerDoor()
    {
        if (enemy.IsDestroyed())
        {
            Destroy(gameObject);
        }
    }
}
