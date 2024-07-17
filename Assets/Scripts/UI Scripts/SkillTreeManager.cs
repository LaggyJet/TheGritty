using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using Unity.VisualScripting;

public class SkillTreeManager : MonoBehaviour, IDataPersistence
{
    public static SkillTreeManager Instance;
    public enum Skills
    {
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

    static readonly List<Tuple<Skills, bool>> skillState = new();

    //Attack
    [Header("------ attack ------")]
    [SerializeField] Image attackDmgLock;
    [SerializeField] Image attackSpdLock;
    [SerializeField] Image staminaUseLock;

    //Ability
    [Header("------ ability ------")]
    [SerializeField] Image ability1Lock;
    [SerializeField] Image ability2Lock;
    [SerializeField] Image ability3Lock;

    //Defense
    [Header("------ defense ------")]
    [SerializeField] Image hpAmtLock;
    [SerializeField] Image unlockShieldLock;
    [SerializeField] Image takeDmgLock;


    //Points
    int curPoints;
    [Header("------ Points ------")]
    [SerializeField] TMP_Text curPointsText;

    //misc.
    [Header("------ misc. ------")]
    private int points2Save;
    [SerializeField] GameObject spawn;

    void Awake()
    {
        Instance = this;
        gameObject.transform.Find("Canvas").gameObject.SetActive(true);
        gameObject.SetActive(false);
    }

    void Start()
    {
        if (skillState.Count != 9)
        {
            foreach (Skills skill in Enum.GetValues(typeof(Skills)))
            {
                skillState.Add(new Tuple<Skills, bool>(skill, false));
            }
        }
        if (DataPersistenceManager.gameData.skills == "" || (GameManager.instance.player.transform.position.x == spawn.transform.position.x && GameManager.instance.player.transform.position.z == spawn.transform.position.z))
        {
            DataPersistenceManager.gameData.skills = "000000000";
            attackDmgLock.enabled = true;
            attackSpdLock.enabled = true;
            staminaUseLock.enabled = true;
            ability1Lock.enabled = true;
            ability2Lock.enabled = true;
            ability3Lock.enabled = true;
            hpAmtLock.enabled = true;
            unlockShieldLock.enabled = true;
            takeDmgLock.enabled = true;
            //FOR TESTING
            curPoints = 8;
            AddPoint();
        }
        else
        {
            LoadData(DataPersistenceManager.gameData);
            if (skillState[0].Item2 == true) { attackDmgLock.enabled = false; }
            if (skillState[1].Item2 == true) { attackSpdLock.enabled = false; }
            if (skillState[2].Item2 == true) { staminaUseLock.enabled = false; }
            if (skillState[3].Item2 == true) { ability1Lock.enabled = false; }
            if (skillState[4].Item2 == true) { ability2Lock.enabled = false; }
            if (skillState[5].Item2 == true) { ability3Lock.enabled = false; }
            if (skillState[6].Item2 == true) { hpAmtLock.enabled = false; }
            if (skillState[7].Item2 == true) { unlockShieldLock.enabled = false; }
            if (skillState[8].Item2 == true) { takeDmgLock.enabled = false; }
        }
    }
    public void AddPoint()
    {
        ++curPoints;
        curPointsText.text = curPoints.ToString("F0");
        points2Save = curPoints;
    }

    public void LosePoint()
    {
        --curPoints;
        curPointsText.text = curPoints.ToString("F0");
        points2Save = curPoints;
    }

    public void LoadSkills(string skills)
    {
        // Ensure skillState has been initialized properly
        if (skillState.Count != skills.Length)
        {
            Debug.LogError("skillState count does not match Skills enum count!");
            return;
        }

        for (int i = 0; i < skillState.Count; i++)
        {
            skillState[i] = new Tuple<Skills, bool>((Skills)i, (skills[i] == '1'));
        }
    }

    public string SaveSkills()
    {
        string state = "";
        for (int i = 0; i < skillState.Count; i++)
            state += skillState[i].Item2 ? '1' : '0';

        return state;
    }

    bool CanUnlockSkill(Skills skill)
    {
        if (curPoints > 0)
        {
            switch (skill)
            {
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

    void UnlockedSkillSound()
    {
        //Whatever here ig
    }

    void CannotUnlockSkillSound()
    {
        //More whatever here
    }

    public void AttackDmg()
    {
        if (CanUnlockSkill(Skills.ATTACK_DAMAGE_UP) && !skillState[0].Item2)
        {
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
        if (CanUnlockSkill(Skills.ATTACK_SPEED_UP) && !skillState[1].Item2)
        {
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
        if (CanUnlockSkill(Skills.STAMINA_USE_DOWN) && !skillState[2].Item2)
        {
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
        if (CanUnlockSkill(Skills.ABILITY_STRENGTH_1) && !skillState[3].Item2)
        {
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
        if (CanUnlockSkill(Skills.ABILITY_STRENGTH_2) && !skillState[4].Item2)
        {
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
        if (CanUnlockSkill(Skills.ABILITY_STRENGTH_3) && !skillState[5].Item2)
        {
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
        if (CanUnlockSkill(Skills.HP_AMOUNT_UP) && !skillState[6].Item2)
        {
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
        if (CanUnlockSkill(Skills.SHIELD) && !skillState[7].Item2)
        {
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
        if (CanUnlockSkill(Skills.DAMAGE_TAKEN_DOWN) && !skillState[8].Item2)
        {
            takeDmgLock.enabled = false;
            skillState[8] = new Tuple<Skills, bool>(skillState[8].Item1, true);
            LosePoint();
            UnlockedSkillSound();
        }
        else
            CannotUnlockSkillSound();
    }

    //load data of a previous game
    public void LoadData(GameData data)
    {
        LoadSkills(data.skills);
        points2Save = data.skillPts;
        curPoints = points2Save;
    }

    //saves all important current data
    public void SaveData(ref GameData data)
    {
        data.skills = SaveSkills();
        data.skillPts = points2Save;
    }
}