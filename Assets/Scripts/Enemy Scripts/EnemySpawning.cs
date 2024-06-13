//Worked on by PJ Glover
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawning : MonoBehaviour
{

    // Start is called before the first frame update
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
