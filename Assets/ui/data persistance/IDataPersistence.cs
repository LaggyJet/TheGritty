//worked on by - natalie lubahn
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDataPersistence
{
    //interface load and save data methods
    void LoadData(GameData data);

    void SaveData(ref GameData data);
}
