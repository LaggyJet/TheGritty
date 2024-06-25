//Worked on by PJ Glover
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawning : MonoBehaviour
{
    [SerializeField] GameObject Enemy;
    [SerializeField] Transform[] SpawnPoints;
    [SerializeField] int numEnemies;

    bool isSpawning;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (!isSpawning)
            {
                isSpawning = true;
                spawn();
            }
        }
    }

    public void spawn()
    {
        for (int i = 0; i < numEnemies; i++)
        {
            int arrayPosition = Random.Range(0, SpawnPoints.Length);
            Instantiate(Enemy, SpawnPoints[arrayPosition].position, SpawnPoints[arrayPosition].rotation);
        }
    }
}
