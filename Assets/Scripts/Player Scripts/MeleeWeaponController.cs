using System.Collections;
using System.Collections.Generic;
using Photon.Realtime;
using UnityEngine;

public class MeleeWeaponController : MonoBehaviour
{
    public float damage_;
    bool canDOT;
    public bool didDamage;
    DamageStats stats_;

    public void SetWeapon(float damage, bool dot, DamageStats type)
    {
        damage_ = damage;
        canDOT = dot;
        stats_ = type;
    }

    private void OnTriggerEnter(Collider other)
    {
        PlayerController.instance.PlayWeaponHit();
        if (other.isTrigger) return;
        
        IDamage damageCheck = other.GetComponent<IDamage>();
        if (damageCheck != null && !other.CompareTag("PlayerChild") && !other.CompareTag("Player") && !didDamage)
        {
            // Play sound when weapon hits enemy 
            PlayerController.instance.PlaySwordHitAud();
            
            if (SkillTreeManager.Instance.IsSkillUnlocked(SkillTreeManager.Skills.ATTACK_DAMAGE_UP))
                damage_ *= 1.5f;

            didDamage = true;
            damageCheck.TakeDamage(damage_);
            if (canDOT)
                damageCheck.Afflict(stats_);
        }
    }
}
