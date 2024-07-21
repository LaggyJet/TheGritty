using System.Collections;
using UnityEngine;
using Photon.Pun;

public class IceBall : MonoBehaviour {
    [SerializeField] Rigidbody rb;
    [SerializeField] float damage;
    [SerializeField] int speed, destroyTime;
    [SerializeField] float minimumLight, maximumLight;
    [SerializeField] new Light light;

    void Start() {
        //moves our projectile forward based on its speed
        rb.velocity = ((FindClosestPlayer().GetComponent<PlayerController>().targetPosition.transform.position - transform.position).normalized) * speed;
        //after being alive so long our projectile will die
        if (PhotonNetwork.InRoom)
            StartCoroutine(WaitThenDestroy(gameObject, destroyTime));
        else if (!PhotonNetwork.InRoom)
            Destroy(gameObject, destroyTime);
        light.intensity = Random.Range(minimumLight, maximumLight);
    }

    // Update is called once per frame
    void Update() { light.intensity = Random.Range(light.intensity + 0.2f, light.intensity - 0.2f); }

    IEnumerator WaitThenDestroy(GameObject obj, float destroyTime) {
        yield return new WaitForSeconds(destroyTime);
        PhotonNetwork.Destroy(obj);
    }

    void OnTriggerEnter(Collider other) {
        //when encountering a collision trigger it checks for component IDamage
        IDamage dmg = other.GetComponent<IDamage>();

        //if there is an IDamage component we run the inside code
        if (dmg != null && (other.gameObject.CompareTag("Player") || other.gameObject.CompareTag("PlayerChild"))) {
            // Sound for collsion 
            PlayerController.instance.PlayIceEnemyAud();
            //deal damage to the object hit
            dmg.TakeDamage(damage);
            //destroy our projectile
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
