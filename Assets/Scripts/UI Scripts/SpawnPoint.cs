//Worked on By : Joshua Furber
using Photon.Pun;
using UnityEngine;

public class SpawnPoint : MonoBehaviourPunCallbacks
{
    public static SpawnPoint Instance { get; private set; }
    public Transform spawnPoint;
    public GameObject player;

    void Start() {
        Instance = this;

        if (PhotonNetwork.InRoom && player != null)
            PhotonNetwork.Instantiate("Player/" + player.name, GetRandomSpawn(), Quaternion.identity);
        else if (!PhotonNetwork.InRoom && player != null)
            Instantiate(player, GetRandomSpawn(), Quaternion.identity);
    }

    Vector3 GetRandomSpawn() { return spawnPoint.position + new Vector3(Random.Range(-2, 2), 0, Random.Range(-2, 2)); }
}