using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwivelDoor : MonoBehaviour
{
    [SerializeField] float openAngle;
    [SerializeField] float closeAngle;
    [SerializeField] float openSpeed;
    [SerializeField] float closeSpeed;
    [SerializeField] int limit;
    int count;
    public bool close = false;
    public bool test;


    private void Update() {
        if (count >= limit|| test)
        {
            OpenDoor();
        }
        else if(close)
        {
            CloseDoor();
        }
    }

    void CloseDoor()
    {
        Quaternion rotation = Quaternion.AngleAxis(closeAngle, transform.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, rotation, closeSpeed);
    }
    void OpenDoor()
    {
        close = false;
        Quaternion rotation = Quaternion.AngleAxis(openAngle, transform.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, rotation, openSpeed);
    }

    public void SwivelTo(float angle_,  float speed_)
    {
        Quaternion rotation = Quaternion.AngleAxis(angle_, transform.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, rotation, speed_);
    }

    public void Increment(int amount)
    {
        count += amount;
    }
}
