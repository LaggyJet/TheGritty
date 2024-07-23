using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using Unity.VisualScripting;
using Photon.Pun;

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
    List<Image> locks;

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
    int curPoints = 0;
    [Header("------ Points ------")]
    [SerializeField] TMP_Text curPointsText;

    //misc.
    [Header("------ misc. ------")]
    private static int points2Save;
    [SerializeField] GameObject spawn;
    bool dataSet = false;

    void Awake()
    {
        Instance = this;
        if (points2Save != 0)
            curPoints = points2Save;
        curPointsText.text = curPoints.ToString("F0");
    }

    void Start()
    {
        locks = new List<Image> { attackDmgLock, attackSpdLock, staminaUseLock, ability1Lock, ability2Lock, ability3Lock, hpAmtLock, unlockShieldLock, takeDmgLock };

        if (skillState.Count != 9)
        {
            foreach (Skills skill in Enum.GetValues(typeof(Skills)))
            {
                skillState.Add(new Tuple<Skills, bool>(skill, false));
            }
        }

        if (SceneManager.GetActiveScene().name == "title menu") return;
        ReloadSkillTreeUI();

    }

    void Update() { if (SceneManager.GetActiveScene().name == "New Build Scene" && !dataSet) SetData(); }

    void SetData() {
        dataSet = true;
        if (!PhotonNetwork.InRoom && DataPersistenceManager.gameData.skills == "" || ((int)GameManager.playerLocation.x == (int)spawn.transform.position.x && (int)GameManager.playerLocation.z == (int)spawn.transform.position.z))
        {
            DataPersistenceManager.gameData.skills = "000000000";
            LoadData(DataPersistenceManager.gameData);
            foreach (Image img in locks)
                img.enabled = true;
        }
        else
        {
            LoadData(DataPersistenceManager.gameData);
            for (int i = 0; i < 9; i++)
                locks[i].enabled = !skillState[i].Item2;
        }
    }

    public void AddPoint()
    {
        PlayerController.instance.PlayAddPointAud();
        ++curPoints;
        curPointsText.text = curPoints.ToString("F0");
        points2Save = curPoints;
    }

    public void LosePoint()
    {
        PlayerController.instance.PlayLosePointAud();
        --curPoints;
        curPointsText.text = curPoints.ToString("F0");
        points2Save = curPoints;
    }

    public void LoadSkills(string skills)
    {
        // Ensure skillState has been initialized properly
        if (skillState.Count != skills.Length)
        {
            return;
        }

        for (int i = 0; i < skillState.Count; i++)
            skillState[i] = new Tuple<Skills, bool>((Skills)i, (skills[i] == '1'));
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
        PlayerController.instance.PlayUnlockSkillAud();
    }

    void CannotUnlockSkillSound()
    {
        PlayerController.instance.PlayCantUnlockSkillAud();
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
        if (PhotonNetwork.InRoom) {
            LoadSkills(data.skills);
            points2Save = data.skillPts;
            curPoints = points2Save;
        }
    }

    //saves all important current data
    public void SaveData(ref GameData data)
    {
        if (PhotonNetwork.InRoom) {
            data.skills = SaveSkills();
            data.skillPts = points2Save;
        }
    }
    void ReloadSkillTreeUI()
    {
        // Load data if not already loaded
        if (!dataSet)
        {
            SetData();
        }
        else
        {
            // Update UI based on skill state
            for (int i = 0; i < 9; i++)
            {
                locks[i].enabled = !skillState[i].Item2;
            }
        }
    }
}