//Worked on By : Joshua Furber
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Arrow : MonoBehaviour
{
    //the body of our projectile that will handle our physics
    [SerializeField] Rigidbody rb;

    //game variables that may be tweaked
    [SerializeField] float damage;
    [SerializeField] int speed;
    [SerializeField] int destroyTime;

    // Start is called before the first frame update
    void Start()
    {
        //moves our projectile forward based on its speed
        rb.velocity = transform.forward * speed;
        //after being alive so long our projectile will die
        if (PhotonNetwork.InRoom)
            StartCoroutine(WaitThenDestroy(gameObject, destroyTime));
        else if (!PhotonNetwork.InRoom)
            Destroy(gameObject, destroyTime);
    }

    IEnumerator WaitThenDestroy(GameObject obj, float destroyTime)
    {
        yield return new WaitForSeconds(destroyTime);
        PhotonNetwork.Destroy(obj);
    }

    private void OnTriggerEnter(Collider other)
    {
        //when encountering a collision trigger it checks for component IDamage
        IDamage dmg = other.GetComponent<IDamage>();

        // Check if arrow hits the head
        if (dmg != null && other.GetComponent<SphereCollider>() != null)
        {
            //deal double damage to the object hit
            dmg.TakeDamage(damage * 2);
            //destroy our projectile
            DestroyObject();
        }
        //if there is an IDamage component we run the inside code
        else if (dmg != null && !other.gameObject.CompareTag("Player"))
        {
            //deal damage to the object hit
            dmg.TakeDamage(damage);
            //destroy our projectile
            DestroyObject();
        }
        else if (!other.gameObject.CompareTag("Player") && !other.isTrigger)
            DestroyObject();

    }

    void DestroyObject()
    {
        if (PhotonNetwork.InRoom && GetComponent<PhotonView>().IsMine)
            PhotonNetwork.Destroy(gameObject);
        else if (!PhotonNetwork.InRoom)
            Destroy(gameObject);
    }
}
