//Made by Emily Underwood
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class BarrelBreaker : MonoBehaviourPunCallbacks
{
    float curTime = 0f;
    float maxTime = 3f;
    bool playerOnBarrel = false;

    private void Update() {
        if (playerOnBarrel) {
            curTime += Time.deltaTime;
            if (curTime >= maxTime)
                DestroyBarrel();
        }
    }

    private void OnTriggerEnter(Collider other) {//box collider will be placed above barrel model
        if (other.CompareTag("Player"))
            playerOnBarrel = true;
    }

    private void OnTriggerExit(Collider other) {
        if (other.CompareTag("Player"))
            playerOnBarrel = false;
    }

    private void DestroyBarrel() {
        if (PhotonNetwork.InRoom)
            PhotonNetwork.Destroy(gameObject);
        else if (!PhotonNetwork.InRoom)
            Destroy(gameObject);
    }
}
