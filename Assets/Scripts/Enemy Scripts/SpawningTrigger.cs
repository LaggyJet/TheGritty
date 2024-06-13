//Worked on by PJ Glover
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawningTrigger : MonoBehaviour
{

    [SerializeField] string nameTag;
    [SerializeField] GameObject SpawnPoint;
    [SerializeField] GameObject Enemy1;
    [SerializeField] int roomOneCount;

    public EnemySpawning spawn;

    // Start is called before the first frame update
    void Start(){}

    // Update is called once per frame
    void Update(){}

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag(nameTag))
        {
            spawn.spawn(roomOneCount, Enemy1, SpawnPoint);
        }
    }
    //private void OnTriggerEnter(Collider other)
    //{
    //    if (other.gameObject.CompareTag("Room 2 Trigger"))
    //    {
    //        spawn(roomTwoCount, Enemy2);
    //    }
    //}
    //private void OnTriggerEnter(Collider other)
    //{
    //    if (other.gameObject.CompareTag("Room 3 Trigger"))
    //    {
    //        spawn(roomThreeCount, Enemy3);
    //    }
    //}
}
