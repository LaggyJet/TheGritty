using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkillTreeManager : MonoBehaviour
{
    //attack
    [Header("------ attack ------")]
    [SerializeField] Image attackDmgLock;
    [SerializeField] Image attackSpdLock;
    [SerializeField] Image staminaUseLock;

    //ability
    [Header("------ ability ------")]
    [SerializeField] Image ability1Lock;
    [SerializeField] Image ability2Lock;
    [SerializeField] Image ability3Lock;

    //defense
    [Header("------ defense ------")]
    [SerializeField] Image hpAmtLock;
    [SerializeField] Image unlockShieldLock;
    [SerializeField] Image takeDmgLock;

    public void attackDmg()
    {
        attackDmgLock.enabled = false;
    }
    public void attackSpd()
    {
        attackSpdLock.enabled = false;
    }
    public void staminaUse()
    {
        staminaUseLock.enabled = false;
    }
    public void ability1()
    {
        ability1Lock.enabled = false;
    }
    public void ability2()
    {
        ability2Lock.enabled = false;
    }
    public void ability3()
    {
        ability3Lock.enabled = false;
    }
    public void hpAmt()
    {
        hpAmtLock.enabled = false;
    }
    public void unlockShield()
    {
        unlockShieldLock.enabled = false;
    }
    public void takeDmg()
    {
        takeDmgLock.enabled = false;
    }
}
