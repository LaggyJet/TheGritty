//Made by Emily Underwood
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BarrelBreaker : MonoBehaviour
{
    private void OnTriggerEnter(Collider other) //box collider will be placed above barrel model
    {
        if (other.CompareTag("Player"))
        {
            Invoke("DestroyBarrel", 3);
        }
    }

    private void DestroyBarrel()
    {
        Destroy(gameObject);
    }
}
