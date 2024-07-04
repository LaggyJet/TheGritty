using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnTrigger : MonoBehaviour
{

    private void OnTriggerEnter(Collider other)
    {
        //if there is an IDamage component we run the inside code
        //if (other.gameObject.CompareTag("Player"))
        //{
        //    GetComponentInChildren<EnemySpawning>().Invoke("spawn", 1f);
        //}

    }
}
