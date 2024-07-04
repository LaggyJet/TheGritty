//Made by Emily Underwood
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class DoorLower : MonoBehaviour
{
    public GameObject door;


    private void Update()
    {
        if (this.GetComponent<EnemyAI>().wasKilled)
        {
            Destroy(door);
        }
    }
}
