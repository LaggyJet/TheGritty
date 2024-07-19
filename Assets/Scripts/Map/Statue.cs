using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Statue : MonoBehaviour, IDamage
{

    [SerializeField] new GameObject light;
    [SerializeField] GameObject door;

    public void TakeDamage(float damage)
    {
        light.SetActive(true);
        door.GetComponent<SwivelDoor>().count++;
    }

    public void Afflict(DamageStats type)
    {
        
    }
}
