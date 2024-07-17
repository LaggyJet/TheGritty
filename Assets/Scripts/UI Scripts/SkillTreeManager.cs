using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SkillTreeManager : MonoBehaviour {
    public static SkillTreeManager Instance;
    public enum Skills {
        ATTACK_DAMAGE_UP = 0,
        ATTACK_SPEED_UP,
        STAMINA_USE_DOWN,
        ABILITY_STRENGTH_1,
        ABILITY_STRENGTH_2,
        ABILITY_STRENGTH_3,
        HP_AMOUNT_UP,
        SHIELD,
        DAMAGE_TAKEN_DOWN
    };

    readonly List<Tuple<Skills, bool>> skillState = new();

    //Attack
    [Header("------ Attack ------")]
    [SerializeField] Image attackDmgLock;
    [SerializeField] Image attackSpdLock;
    [SerializeField] Image staminaUseLock;

    //Ability
    [Header("------ Ability ------")]
    [SerializeField] Image ability1Lock;
    [SerializeField] Image ability2Lock;
    [SerializeField] Image ability3Lock;

    //Defense
    [Header("------ Defense ------")]
    [SerializeField] Image hpAmtLock;
    [SerializeField] Image unlockShieldLock;
    [SerializeField] Image takeDmgLock;


    //Points
    int curPoints;
    [Header("------ Points ------")]
    [SerializeField] TMP_Text curPointsText;

    void Awake() {
        Instance = this;
        gameObject.transform.Find("Canvas").gameObject.SetActive(true);
        gameObject.SetActive(false);
    }

    void Start() {
        foreach (Skills skill in Enum.GetValues(typeof(Skills))) {
            skillState.Add(new Tuple<Skills, bool>(skill, false));
            AddPoint();
        }
    }

    public void AddPoint() {
        ++curPoints; 
        curPointsText.text = curPoints.ToString("F0");
    }

    public void LosePoint() {
        --curPoints;
        curPointsText.text = curPoints.ToString("F0");
    }

    bool CanUnlockSkill(Skills skill) {
        if (curPoints > 0) {
            switch (skill) {
                case Skills.ATTACK_DAMAGE_UP:
                case Skills.ABILITY_STRENGTH_1:
                case Skills.HP_AMOUNT_UP:
                    return true;
                case Skills.ATTACK_SPEED_UP:
                case Skills.STAMINA_USE_DOWN:
                    return (skillState[0].Item2);
                case Skills.ABILITY_STRENGTH_2:
                    return (skillState[3].Item2);
                case Skills.ABILITY_STRENGTH_3:
                    return (skillState[4].Item2);
                case Skills.DAMAGE_TAKEN_DOWN:
                case Skills.SHIELD:
                    return (skillState[6].Item2);
            }
        }
        return false;
    }

    public bool IsSkillUnlocked(Skills skill) { return skillState.Any(tuple => tuple.Item1.Equals(skill) && tuple.Item2); }

    void UnlockedSkillSound() {
        //Whatever here ig
    }

    void CannotUnlockSkillSound() {
        //More whatever here
    }

    public void AttackDmg()
    {
        if (CanUnlockSkill(Skills.ATTACK_DAMAGE_UP) && !skillState[0].Item2) {
            attackDmgLock.enabled = false;
            skillState[0] = new Tuple<Skills, bool>(skillState[0].Item1, true);
            LosePoint();
            UnlockedSkillSound();
        }
        else
            CannotUnlockSkillSound();
    }
    public void AttackSpd()
    {
        if (CanUnlockSkill(Skills.ATTACK_SPEED_UP) && !skillState[1].Item2) {
            attackSpdLock.enabled = false;
            skillState[1] = new Tuple<Skills, bool>(skillState[1].Item1, true);
            LosePoint();
            UnlockedSkillSound();
        }
        else
            CannotUnlockSkillSound();
    }
    public void StaminaUse()
    {
        if (CanUnlockSkill(Skills.STAMINA_USE_DOWN) && !skillState[2].Item2) {
            staminaUseLock.enabled = false;
            skillState[2] = new Tuple<Skills, bool>(skillState[2].Item1, true);
            LosePoint();
            UnlockedSkillSound();
        }
        else
            CannotUnlockSkillSound();
    }
    public void Ability1()
    {
        if (CanUnlockSkill(Skills.ABILITY_STRENGTH_1) && !skillState[3].Item2) {
            ability1Lock.enabled = false;
            skillState[3] = new Tuple<Skills, bool>(skillState[3].Item1, true);
            LosePoint();
            UnlockedSkillSound();
        }
        else
            CannotUnlockSkillSound();
    }
    public void Ability2()
    {
        if (CanUnlockSkill(Skills.ABILITY_STRENGTH_2) && !skillState[4].Item2) {
            ability2Lock.enabled = false;
            skillState[4] = new Tuple<Skills, bool>(skillState[4].Item1, true);
            LosePoint();
            UnlockedSkillSound();
        }
        else
            CannotUnlockSkillSound();
    }
    public void Ability3()
    {
        if (CanUnlockSkill(Skills.ABILITY_STRENGTH_3) && !skillState[5].Item2) {
            ability3Lock.enabled = false;
            skillState[5] = new Tuple<Skills, bool>(skillState[5].Item1, true);
            LosePoint();
            UnlockedSkillSound();
        }
        else
            CannotUnlockSkillSound();
    }
    public void HpAmt()
    {
        if (CanUnlockSkill(Skills.HP_AMOUNT_UP) && !skillState[6].Item2) {
            hpAmtLock.enabled = false;
            skillState[6] = new Tuple<Skills, bool>(skillState[6].Item1, true);
            LosePoint();
            UnlockedSkillSound();
        }
        else
            CannotUnlockSkillSound();
    }
    public void UnlockShield()
    {
        if (CanUnlockSkill(Skills.SHIELD) && !skillState[7].Item2) {
            unlockShieldLock.enabled = false;
            skillState[7] = new Tuple<Skills, bool>(skillState[7].Item1, true);
            LosePoint();
            UnlockedSkillSound();
        }
        else
            CannotUnlockSkillSound();
    }
    public void TakeDmg()
    {
        if (CanUnlockSkill(Skills.DAMAGE_TAKEN_DOWN) && !skillState[8].Item2) {
            takeDmgLock.enabled = false;
            skillState[8] = new Tuple<Skills, bool>(skillState[8].Item1, true);
            LosePoint();
            UnlockedSkillSound();
        }
        else
            CannotUnlockSkillSound();
    }
}