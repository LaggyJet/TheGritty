//Worked on by PJ Glover, Joshua Furber
using Photon.Pun;
using UnityEngine;

public class EnemySpawning : MonoBehaviourPunCallbacks {
    [SerializeField] GameObject Enemy;
    [SerializeField] Transform[] SpawnPoints;
    [SerializeField] int numEnemies;

    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Player") && !GameManager.instance.spawnedEnemies && other.GetComponent<PhotonView>().IsMine)
            Spawn();
    }

    public void Spawn() {
        GameManager.instance.spawnedEnemies = true;
        for (int i = 0; i < numEnemies; i++) {
            int arrayPosition = Random.Range(0, SpawnPoints.Length);
            if (PhotonNetwork.InRoom && Enemy != null)
                PhotonNetwork.Instantiate("Enemy/" + Enemy.name, SpawnPoints[arrayPosition].position, SpawnPoints[arrayPosition].rotation);
            else if (!PhotonNetwork.InRoom && Enemy != null)
                Instantiate(Enemy, SpawnPoints[arrayPosition].position, SpawnPoints[arrayPosition].rotation);
        }
    }
}
