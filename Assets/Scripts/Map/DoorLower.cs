//Made by Emily Underwood
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorLower : MonoBehaviour
{

    public GameObject door;
    public int numEnemiesThreshold; //number of enemies to kill in order to lower door
    public int enemiesKilled = 0;


    public void lowerDoor()
    {
        if (enemiesKilled >= numEnemiesThreshold)
        {
            Destroy(door);
        }
    }

    public void increaseEnemiesKilled()
    {
        enemiesKilled++;
    }
}
