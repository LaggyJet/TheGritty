using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwivelDoor : MonoBehaviour
{
    [SerializeField] public float angle;
    [SerializeField] public float speed;
    [SerializeField] public int limit;
    public int count;
    public bool swivel;

    private void Update()
    {
        if (count == limit)
        {
            Swivel();
        }
        if (swivel)
        {
            Swivel();
        }
    }

    public void Swivel()
    {
        Quaternion rotation = Quaternion.AngleAxis(angle, transform.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, rotation, speed);
    }
}
