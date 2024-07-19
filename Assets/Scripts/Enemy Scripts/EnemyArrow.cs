using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Unity.VisualScripting;

public class EnemyArrow : MonoBehaviour {
    [SerializeField] Rigidbody rb;
    [SerializeField] float damage, speed, destroyTime;

    void Start() { 
        rb.velocity = ((FindClosestPlayer().GetComponent<PlayerController>().targetPosition.transform.position - transform.position).normalized) * speed;
        if (PhotonNetwork.InRoom)
            StartCoroutine(WaitThenDestroy(gameObject, destroyTime));
        else if (!PhotonNetwork.InRoom)
            Destroy(gameObject, destroyTime);
    }

    void Update() { transform.rotation = Quaternion.LookRotation(rb.velocity.normalized); }

    IEnumerator WaitThenDestroy(GameObject obj, float destroyTime) {
        yield return new WaitForSeconds(destroyTime);
        PhotonNetwork.Destroy(obj);
    }

    void OnTriggerEnter(Collider other) {
        IDamage dmg = other.GetComponent<IDamage>();

        if (dmg != null && (other.gameObject.CompareTag("Player") || other.gameObject.CompareTag("PlayerChild"))) {
            dmg.TakeDamage(damage);
            DestroyObject();
        }
        else if ((other.gameObject.CompareTag("Player") || other.gameObject.CompareTag("PlayerChild")) && !other.isTrigger)
            DestroyObject();
    }

    void DestroyObject() {
        if (PhotonNetwork.InRoom && GetComponent<PhotonView>().IsMine)
            PhotonNetwork.Destroy(gameObject);
        else if (!PhotonNetwork.InRoom)
            Destroy(gameObject);
    }

    GameObject FindClosestPlayer() {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        GameObject closestPlayer = null;
        float closestDistance = Mathf.Infinity;

        foreach (GameObject player in players) {
            float distance = Vector3.Distance(transform.position, player.transform.position);
            if (distance < closestDistance) {
                closestDistance = distance;
                closestPlayer = player;
            }
        }

        return closestPlayer;
    }
}
