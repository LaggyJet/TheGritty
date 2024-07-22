//worked on by - natalie lubahn
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[System.Serializable]
public class GameData
{
    //public variables for serializing data from the other scripts
    public Vector3 playerPos;
    public Quaternion playerRot;
    public float playerHp;
    public float playerStamina;
    public string skills = "000000000";
    public int skillPts;
    public bool hasBossDied;

    public GameData()  //constructor - values are the initial values when starting a game
    {
            this.playerPos = Vector3.zero;
            this.playerRot = Quaternion.identity;
            this.playerHp = 0;
            this.playerStamina = 0;
            this.skillPts = 0;
            this.hasBossDied = false;

    }
}