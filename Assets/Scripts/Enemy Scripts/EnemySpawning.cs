//Worked on by PJ Glover
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawning : MonoBehaviour
{

    void Start()
    {
    }

    // Update is called once per frame
    void Update() {}

    public void spawn(int numEnemies, GameObject Enemy, GameObject spawnLocation)
    {
        for (int i = 0; i < numEnemies; i++)
        {
            Instantiate(Enemy, spawnLocation.transform.position, transform.rotation);
        }
    }

    
}
