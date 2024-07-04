//Made by Emily Underwood
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HPotPickUp : MonoBehaviour
{
    [SerializeField] float hpAdd; //amount of hp the potion adds to the player
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            GameManager.instance.playerScript.AddHP(hpAdd);
            Destroy(gameObject);
        }
    }
}
