//Made by Emily Underwood
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class BarrelBreaker : MonoBehaviourPunCallbacks
{
    private void OnTriggerEnter(Collider other) {//box collider will be placed above barrel model
        if (other.CompareTag("Player"))
            Invoke(nameof(DestroyBarrel), 3);
    }

    private void DestroyBarrel() {
        if (PhotonNetwork.InRoom)
            PhotonNetwork.Destroy(gameObject);
        else if (!PhotonNetwork.InRoom)
            Destroy(gameObject);
    }
}
