//Worked on by PJ Glover
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawning : MonoBehaviour
{
    [SerializeField] GameObject Enemy;
    [SerializeField] GameObject SpawnPoint;
    [SerializeField] int numEnemies;

    // Start is called before the first frame update
    void Start() 
    {
        spawn();
    }

    // Update is called once per frame
    void Update() {}

    void spawn()
    {
        for (int i = 0; i < numEnemies; i++)
        {
            Instantiate(Enemy, SpawnPoint.transform.position, transform.rotation);
        }
    }
}
