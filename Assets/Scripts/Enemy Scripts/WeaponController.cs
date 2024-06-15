// Worked on by - Joshua Furber
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponController : MonoBehaviour {
    int damage_;
    public void SetDamage(int damage) { damage_ = damage; }

    private void OnTriggerEnter(Collider other) {
        if (other.isTrigger) return;

        IDamage damageCheck = other.GetComponent<IDamage>();
        if (damageCheck != null && other.CompareTag("Player"))
            damageCheck.TakeDamage(damage_);
    }
}