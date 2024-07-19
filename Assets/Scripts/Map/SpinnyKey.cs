using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SpinnyKey : MonoBehaviour
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

    public void OnCollisionEnter(Collision collision)
    {
        door.GetComponent<SwivelDoor>().swivel = true;
        Destroy(gameObject);
    }
}
