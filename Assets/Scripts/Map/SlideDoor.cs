//Made by Joshua Furber
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlideDoor : MonoBehaviour {
    [SerializeField] GameObject leftDoor, rightDoor;
    [SerializeField] GameObject[] enemies;
    [SerializeField] int killThreshold;
    [SerializeField] float openingSpeed;

    readonly List<EnemyLimiter> curLimiters = new();

    void Start() {
        for (int i = 0; i <  enemies.Length; i++)
            curLimiters.Add(enemies[i].GetComponent<EnemyAI>().GetEnemyLimiter());
    }

    void Update() {
        int currentKills = 0;
        foreach (EnemyLimiter limiter in curLimiters)
            if (EnemyManager.Instance.GetEnemyIndex(limiter) != -1)
                currentKills += EnemyManager.Instance.GetKilledEnemyCount(limiter);

        if (currentKills >= killThreshold)
            StartCoroutine(OpenDoor());
    }

    IEnumerator OpenDoor() {
        foreach (EnemyLimiter limiter in curLimiters)
            EnemyManager.Instance.ResetKillCounter(limiter);

        float leftEnd = leftDoor.transform.position.z - 1.3f;
        float rightEnd = rightDoor.transform.position.z + 1.3f;
    
        while (leftDoor.transform.position.z > leftEnd || rightDoor.transform.position.z < rightEnd) {
            Vector3 leftDoorPosition = leftDoor.transform.position;
            Vector3 rightDoorPosition = rightDoor.transform.position;
        
            if (leftDoorPosition.z > leftEnd) {
                leftDoorPosition.z -= openingSpeed * Time.deltaTime;
                leftDoor.transform.position = leftDoorPosition;
            }

            if (rightDoorPosition.z < rightEnd) {
                rightDoorPosition.z += openingSpeed * Time.deltaTime;
                rightDoor.transform.position = rightDoorPosition;
            }

            yield return null;
        }
    }
}
