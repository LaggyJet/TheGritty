//worked on by - natalie lubahn
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SavePoint : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        GameManager.instance.ShowText("Saving...");
        DataPersistenceManager.Instance.SaveGame();
        GameManager.playerLocation = other.transform.position;
    }
}
