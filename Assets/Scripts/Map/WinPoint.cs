// Written by Emily Underwood

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WinPoint : MonoBehaviour
{
    public GameObject Player;

    private void OnTriggerEnter(Collider collide)
    {
        if (collide.gameObject.CompareTag("Player"))
        {
            GameManager.instance.gameWon();
        }
    }
}
