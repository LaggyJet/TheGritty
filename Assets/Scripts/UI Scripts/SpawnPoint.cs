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

        if (PhotonNetwork.InLobby && player != null)
            PhotonNetwork.Instantiate("Player/" + player.name, spawnPoint.position, Quaternion.identity);
        else if (!PhotonNetwork.InLobby)
            Instantiate(player, spawnPoint.position, Quaternion.identity);
    }
}