// Worked on by - Joshua Furber
using UnityEngine;

public class AcidPuddle : MonoBehaviour {
    [SerializeField] DamageStats stats_;

    private void Start() { Destroy(gameObject, 4); }

    private void OnTriggerEnter(Collider other) {
        IDamage damageCheck = other.GetComponent<IDamage>();
        if (damageCheck != null && other.CompareTag("Player"))
            damageCheck.Afflict(stats_);
    }
}
