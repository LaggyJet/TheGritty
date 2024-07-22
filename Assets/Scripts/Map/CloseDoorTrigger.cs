using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloseDoorTrigger : MonoBehaviour, I_Interact
{
    [SerializeField] GameObject[] doors;
    public bool hasClosed = false;

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.CompareTag("Player") && !hasClosed)
            CallDoor();
    }

    public void CallDoor()
    {
        hasClosed = true;
        foreach (GameObject object_ in doors)
            object_.GetComponent<SwivelDoor>().close = true;
    }

    public void PassGameObject(GameObject object_) { }
}
