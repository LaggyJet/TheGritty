//Worked on By - Joshua Furber
using Photon.Pun;
using UnityEngine;

public class SpawnPoint : MonoBehaviourPunCallbacks
{
    public static SpawnPoint instance { get; private set; }
    public Transform spawnPoint;
    public GameObject player;

    void Start()
    {
        instance = this;

        if (PhotonNetwork.IsConnected)
            InstantiatePlayer();
    }

    void InstantiatePlayer() {
        if (player != null)
            PhotonNetwork.Instantiate(player.name, spawnPoint.position, Quaternion.identity);
    }
}