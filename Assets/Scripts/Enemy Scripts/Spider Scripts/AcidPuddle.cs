// Worked on by - Joshua Furber
using UnityEngine;
using Photon.Pun;
using System.Collections;

public class AcidPuddle : MonoBehaviourPunCallbacks {
    [SerializeField] DamageStats stats_;

    private void Start() {
        if (PhotonNetwork.InRoom)
            StartCoroutine(WaitThenDestroy(gameObject, 4));
        else if (!PhotonNetwork.InRoom)
            Destroy(gameObject, 4); 
    }

    IEnumerator WaitThenDestroy(GameObject obj, float seconds) {
        yield return new WaitForSeconds(seconds);
        PhotonNetwork.Destroy(obj);
    }

    private void OnTriggerEnter(Collider other) {
        IDamage damageCheck = other.GetComponent<IDamage>();
        if (damageCheck != null && other.CompareTag("Player"))
            damageCheck.Afflict(stats_);
    }
}
