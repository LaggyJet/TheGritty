using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Statue : MonoBehaviour, IDamage
{

    [SerializeField] new GameObject light;
    [SerializeField] GameObject[] doors;
    bool hit = false;

    public void TakeDamage(float damage)
    {
        if(!hit)
        {
            hit = true;
            light.SetActive(true);
            foreach (GameObject object_ in doors)
            {
                object_.GetComponent<SwivelDoor>().Increment(1);
            }
        }
        
    }

    public void Afflict(DamageStats type)
    {
        
    }
}
