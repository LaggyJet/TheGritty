//Made by Emily Underwood
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class HPotPickUp : MonoBehaviourPunCallbacks
{
    [SerializeField] float hpAdd; //amount of hp/stamina the potion adds to the player
    [SerializeField] float staminaAdd; 
    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Player")) {
            if (other.GetComponent<PhotonView>().IsMine || !PhotonNetwork.IsConnected)
                GameManager.instance.playerScript.AddHP(hpAdd);
            if (PhotonNetwork.InRoom && other.GetComponent<PhotonView>().IsMine)
                PhotonNetwork.Destroy(gameObject);
            else if (!PhotonNetwork.InRoom)
                Destroy(gameObject);
        }
    }
// Need to adjust script to accept stamina potions 

}
