// Written by Emily Underwood

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WinPoint : MonoBehaviour
{

    private void Update()
    {
        if (this.GetComponent<SpiderController>().wasKilled)
        {
            Invoke("Win", 2); //done to prevent glitchy menu
        }
    }

    private void Win()
    {
        GameManager.instance.gameWon();
    }
}
