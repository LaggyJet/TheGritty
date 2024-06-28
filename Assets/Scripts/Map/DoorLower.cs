//Made by Emily Underwood
using System.Collections;
using UnityEngine;

public class DoorLower : MonoBehaviour
{
    public GameObject leftDoor, rightDoor, enemy;
    public int killThreshold;
    public float openingSpeed;

    void Update() {
        EnemyLimiter curLimiter = enemy.GetComponent<EnemyAI>().GetEnemyLimiter();
        if (EnemyManager.Instance.GetEnemyIndex(curLimiter) != -1 && EnemyManager.Instance.GetKilledEnemyCount(curLimiter) >= killThreshold) {
            EnemyManager.Instance.ResetKillCounter(curLimiter);
            StartCoroutine(OpenDoor());
        }
    }

    IEnumerator OpenDoor() {
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
