// Worked on by - Joshua Furber
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponController : MonoBehaviour {
    [SerializeField] GameObject weapon;
    [SerializeField] int damage;

    private void OnTriggerEnter(Collider other) {
        if (other.isTrigger) return;

        IDamage damageCheck = other.GetComponent<IDamage>();
        if (damageCheck != null)
            damageCheck.TakeDamage(damage);
    }
}
