// Worked on by - Joshua Furber
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AcidPuddle : MonoBehaviour {
    DamageStats stats_;

    public void SetDamageType(DamageStats type) { stats_ = type; }

    private void OnParticleCollision(GameObject other) {
        IDamage damageCheck = other.GetComponent<IDamage>();
        if (damageCheck != null && other.CompareTag("Player"))
            damageCheck.Afflict(stats_);
    }
}
