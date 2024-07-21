using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SpinnyKey : MonoBehaviour, I_Interact
{
    [SerializeField] GameObject door;
    bool spinning;
    float angle = 0;

    private void Update()
    {
        StartCoroutine(StartSpin());
    }

    IEnumerator StartSpin() {
        if(!spinning)
        {
            spinning = true;
            angle += 10;
            Quaternion rotation = Quaternion.AngleAxis(angle, transform.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, rotation, .8f);
            yield return new WaitForSeconds(0.05f);
            spinning = false;
        }
    }

    public void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.CompareTag("Player"))
        {
            SkillTreeManager.Instance.AddPoint();
            CallDoor();
            Destroy(gameObject);
        }
        
    }

    public void CallDoor()
    {
        door.GetComponent<SwivelDoor>().Increment(1);
    }

    public void PassGameObject(GameObject object_)
    {
        door = object_;
    }
}
