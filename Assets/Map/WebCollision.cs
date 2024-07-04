
//made by Emily Underwood
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WebCollision : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            GameManager.instance.playerScript.Slow();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            GameManager.instance.playerScript.UnSlow();
        }
    }
}
