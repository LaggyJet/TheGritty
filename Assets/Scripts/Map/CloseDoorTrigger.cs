using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloseDoorTrigger : MonoBehaviour, I_Interact
{
    [SerializeField] GameObject[] doors;

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.CompareTag("Player"))
            CallDoor();
    }

    public void CallDoor()
    {
        foreach (GameObject object_ in doors)
            object_.GetComponent<SwivelDoor>().close = true;
    }

    public void PassGameObject(GameObject object_) { }
}
