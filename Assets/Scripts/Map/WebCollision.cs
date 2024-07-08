
//made by Emily Underwood
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WebCollision : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && (other.GetComponent<PhotonView>().IsMine || !PhotonNetwork.IsConnected))
        {
            GameManager.instance.playerScript.Slow();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && (other.GetComponent<PhotonView>().IsMine || !PhotonNetwork.IsConnected))
        {
            GameManager.instance.playerScript.UnSlow();
        }
    }
}
