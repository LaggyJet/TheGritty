using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    public static SpawnPoint instance { get; private set; }
    public Transform spawnPoint;

    public void Spawn()
    {
        transform.position = spawnPoint.position;
    }
}
