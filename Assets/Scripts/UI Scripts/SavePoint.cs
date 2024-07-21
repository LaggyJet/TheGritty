//worked on by - natalie lubahn
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Photon.Pun;

public class SavePoint : MonoBehaviour
{
    private bool activated = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player" && !activated && !PhotonNetwork.InRoom)
        {
            GameManager.instance.ShowText("Saving...");
            DataPersistenceManager.Instance.SaveGame();
            GameManager.playerLocation = other.transform.position;
            activated = true;
            GameManager.instance.SoundTrackswitch(GameManager.GameMusic.Gameplay);
        }
    }
}
