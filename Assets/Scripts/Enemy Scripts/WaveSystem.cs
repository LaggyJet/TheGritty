//Worked on by PJ Glover
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveSystem : MonoBehaviour
{
    [SerializeField] GameObject boss;
    [SerializeField] GameObject[] Enemies;
    [SerializeField] Transform[] SpawnPoints;
    [SerializeField] int baseEnemyNum;
    [SerializeField] int enemyMultiplier;
    [SerializeField] int delayBetweenRounds;


    int currentSpawnEnemies;
    bool isWavesRunning;

    private void Start()
    {
        currentSpawnEnemies = baseEnemyNum;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isWavesRunning)
        {
            StartCoroutine(StartWaves());
        }
    }

    IEnumerator StartWaves()
    {
        isWavesRunning = true;
        while (boss != null)
        {
            int randEnemyPosition = Random.Range(0, Enemies.Length);
            int randSpawnPosition = Random.Range(0, SpawnPoints.Length);
            for (int i = 0; i < currentSpawnEnemies; i++)
            {
                Instantiate(Enemies[randEnemyPosition], SpawnPoints[randSpawnPosition].position, SpawnPoints[randSpawnPosition].rotation);
                yield return null;
            }
            currentSpawnEnemies *= enemyMultiplier;
            yield return new WaitForSeconds(delayBetweenRounds);
        }
    }
}
