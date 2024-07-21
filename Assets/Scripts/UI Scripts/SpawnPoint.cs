//Worked on By : Joshua Furber
using Photon.Pun;
using Unity.VisualScripting;
using UnityEngine;

public class SpawnPoint : MonoBehaviourPunCallbacks
{
    public GameObject player;

    void Start() {

        if (PhotonNetwork.InRoom && player != null)
            PhotonNetwork.Instantiate("Player/" + player.name, GetRandomSpawn(), Quaternion.identity);
        else if (!PhotonNetwork.InRoom && player != null) {
            Instantiate(player, gameObject.transform.position, Quaternion.identity);
            GameManager.playerLocation = gameObject.transform.position;
        }
    }

    Vector3 GetRandomSpawn() { return gameObject.transform.position + new Vector3(Random.Range(-4, 4), 0, Random.Range(-4, 4)); }
}