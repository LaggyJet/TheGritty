//Written by Emily Underwood

using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Teleporter : MonoBehaviour
{
    public GameObject Player;
    public GameObject TPPoint;

    private void OnTriggerEnter(Collider collide)
    {
        if (collide.gameObject.CompareTag("Player"))
        {
            Player.SetActive(false);
            Player.transform.position = TPPoint.transform.position;
            Player.SetActive(true);
        }
    }
}
