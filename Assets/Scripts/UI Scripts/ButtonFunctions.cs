//worked on by - natalie lubahn, Jacob Irvin
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;

public class ButtonFunctions : MonoBehaviour
{
    [SerializeField] private ClassSelection playerClass;

    public void resume()
    {
        GameManager.instance.stateResume();
    }
    public void restart()
    {
        GameManager.enemyCount = 0;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        PlayerController.spawnHP = 10;
        GameManager.instance.playerScript.UpdatePlayerUI();
        GameManager.instance.stateResume();
    }
    public void quitApp()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void quitGame()
    {
        SceneManager.LoadScene("title menu");

        //Disconnect player from the server (if possible)
        if (PhotonNetwork.IsConnected)
            PhotonNetwork.Disconnect();
    }

    public void respawn()
    {
        GameManager.instance.respawnAfterLost();
    }
    public void settings()
    {
        GameManager.instance.openSettings();
    }
    public void quitSettings()
    {
        GameManager.instance.leaveSettings();
        if(SettingsManager.instance != null)
        SettingsManager.instance.SavePrefs();
        ResolutionManager.instance.SavePrefs();
    }

    public void quitSaveWarning()
    {
        DataPersistenceManager.gameData = DataPersistenceManager.Instance.dataHandler.Load();
        if (DataPersistenceManager.gameData.playerPos != GameManager.instance.player.transform.position || DataPersistenceManager.gameData.playerHp != GameManager.instance.playerScript.currentHP || DataPersistenceManager.gameData.playerStamina != GameManager.instance.playerScript.currentStamina)
        {
            GameManager.instance.Warning4SaveProgress();
        }
        else
        {
            quitGame();
        }
    }

    //FOR TITLE SCREEN
    public void newGame()
    {
        SceneManager.LoadScene("Build Scene");
        DataPersistenceManager.Instance.NewGame();
        GameManager.instance.stateResumeGameLoads();
    }
    public void loadGame()
    {
        SceneManager.LoadScene("Build Scene");
        DataPersistenceManager.Instance.LoadGame();
        GameManager.instance.stateResumeGameLoads();
    }
    public void startNewGamePart1()
    {
        GameManager.instance.Warning4NewGame();
    }
    public void startNewGamePart2()
    {
        GameManager.instance.charSelectionMenu();
    }

    //CLASS SELECTION
    public void warrior()
    {
        playerClass.MyClass = 1;
        newGame();
    }
    public void mage()
    {
        playerClass.MyClass = 2;
        newGame();

    }
    public void archer()
    {
        playerClass.MyClass = 3;
        newGame();
    }

    // Co-op features
    public void LoadMultiplayer() { SceneManager.LoadScene("Lobby"); }
}
