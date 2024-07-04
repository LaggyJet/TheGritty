//Made by Emily Underwood
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class HPotPickUp : MonoBehaviourPunCallbacks
{
    [SerializeField] float hpAdd; //amount of hp the potion adds to the player
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            GameManager.instance.playerScript.AddHP(hpAdd);
            if (PhotonNetwork.InRoom)
                PhotonNetwork.Destroy(gameObject);
            else if (!PhotonNetwork.InRoom)
                Destroy(gameObject);
        }
    }
}
