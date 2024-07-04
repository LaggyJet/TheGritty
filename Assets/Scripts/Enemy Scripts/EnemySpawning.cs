//Worked on by PJ Glover, Joshua Furber
using Photon.Pun;
using UnityEngine;

public class EnemySpawning : MonoBehaviourPunCallbacks {
    [SerializeField] GameObject Enemy;
    [SerializeField] Transform[] SpawnPoints;
    [SerializeField] int numEnemies;

    bool isSpawning;

    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Player") && !isSpawning)
            Spawn();
    }

    public void Spawn() {
        isSpawning = true;
        for (int i = 0; i < numEnemies; i++) {
            int arrayPosition = Random.Range(0, SpawnPoints.Length);
            if (PhotonNetwork.InRoom)
                PhotonNetwork.Instantiate("Enemy/" + Enemy.name, SpawnPoints[arrayPosition].position, SpawnPoints[arrayPosition].rotation);
            else if (!PhotonNetwork.InRoom)
                Instantiate(Enemy, SpawnPoints[arrayPosition].position, SpawnPoints[arrayPosition].rotation);
        }
    }
}
