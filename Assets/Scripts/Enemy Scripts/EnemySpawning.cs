//Worked on by PJ Glover, Joshua Furber
using Photon.Pun;
using UnityEngine;

public class EnemySpawning : MonoBehaviourPunCallbacks, IPunObservable {
    [SerializeField] GameObject Enemy;
    [SerializeField] Transform[] SpawnPoints;
    [SerializeField] int numEnemies;

    bool isSpawning;

    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Player") && !isSpawning && other.GetComponent<PhotonView>().IsMine)
            Spawn();
    }

    public void Spawn() {
        isSpawning = true;
        for (int i = 0; i < numEnemies; i++) {
            int arrayPosition = Random.Range(0, SpawnPoints.Length);
            if (PhotonNetwork.InRoom && Enemy != null)
                PhotonNetwork.Instantiate("Enemy/" + Enemy.name, SpawnPoints[arrayPosition].position, SpawnPoints[arrayPosition].rotation);
            else if (!PhotonNetwork.InRoom && Enemy != null)
                Instantiate(Enemy, SpawnPoints[arrayPosition].position, SpawnPoints[arrayPosition].rotation);
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
        if (stream.IsWriting)
            stream.SendNext(isSpawning);
        else
            isSpawning = (bool)stream.ReceiveNext();
    }
}
