//Written by Emily Underwood

using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Teleporter : MonoBehaviour
{
    public GameObject TPPoint;

    private void OnTriggerEnter(Collider collide)
    {
        if (collide.gameObject.CompareTag("Player"))
        {
            EnemyManager.Instance.ClearEnemies();
            PlayerController.instance.PlayTeleport();
            GameManager.instance.playerScript.controller.enabled = false;
            GameManager.instance.player.transform.position = TPPoint.transform.position;
            GameManager.playerLocation = GameManager.instance.player.transform.position;
            GameManager.instance.playerScript.controller.enabled = true;
            PlayerController.instance.isPlayingTeleport = false;
        }
    }
}
