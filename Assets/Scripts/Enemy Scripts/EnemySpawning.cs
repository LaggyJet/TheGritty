//Worked on by PJ Glover, Joshua Furber
using Photon.Pun;
using UnityEngine;

public class EnemySpawning : MonoBehaviour {
    [SerializeField] GameObject Enemy;
    [SerializeField] Transform[] SpawnPoints;
    [SerializeField] int numEnemies;
    [SerializeField] GameObject[] doors;

    bool isSpawning = false;

    [PunRPC]
    void UpdateSpawnerState() { isSpawning = true; }

    [PunRPC]
    public void Spawn(Vector3 playerPosition) {
        isSpawning = true;

        if (PhotonNetwork.InRoom)
            GetComponent<PhotonView>().RPC(nameof(UpdateSpawnerState), RpcTarget.Others);

        if (!PhotonNetwork.IsMasterClient && PhotonNetwork.InRoom)
            GetComponent<PhotonView>().RPC(nameof(Spawn), RpcTarget.MasterClient, playerPosition);

        if (PhotonNetwork.IsMasterClient || !PhotonNetwork.InRoom) {
            for (int i = 0; i < numEnemies; i++) {
                int arrayPosition = Random.Range(0, SpawnPoints.Length);
                Vector3 spawnPos = GetRandomSpawn(SpawnPoints[arrayPosition].position);
                Quaternion spawnRot = Quaternion.LookRotation(playerPosition - spawnPos);

                if (PhotonNetwork.InRoom && Enemy != null)
                {
                    GameObject temp = PhotonNetwork.Instantiate("Enemy/" + Enemy.name, spawnPos, spawnRot);
                    foreach(GameObject object_ in doors)
                        temp.GetComponent<I_Interact>().PassGameObject(object_);
                }
                else if (!PhotonNetwork.InRoom && Enemy != null)
                {
                    GameObject temp = Instantiate(Enemy, spawnPos, spawnRot);
                    foreach (GameObject object_ in doors)
                        temp.GetComponent<I_Interact>().PassGameObject(object_);
                }
            }
        }
    }

    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Player") && !isSpawning && (other.GetComponent<PhotonView>().IsMine || !PhotonNetwork.InRoom)) {
            Vector3 playerPosition = other.transform.position;
            Spawn(playerPosition);
        }
    }

    Vector3 GetRandomSpawn(Vector3 curPos) { return curPos + new Vector3(Random.Range(-2, 2), 0, Random.Range(-2, 2)); }
}
