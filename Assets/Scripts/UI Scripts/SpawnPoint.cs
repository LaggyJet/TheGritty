//Worked on By - Joshua Furber
using Photon.Pun;
using UnityEngine;

public class SpawnPoint : MonoBehaviourPunCallbacks
{
    public static SpawnPoint Instance { get; private set; }
    public Transform spawnPoint;
    public GameObject player;

    void Start() {
        Instance = this;

        if (PhotonNetwork.IsConnected && player != null)
            PhotonNetwork.Instantiate(player.name, spawnPoint.position, Quaternion.identity);
    }
}