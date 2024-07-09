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
        if ((int)GameManager.playerLocation.x != (int)GameManager.instance.player.transform.position.x && (int)GameManager.playerLocation.z != (int)GameManager.instance.player.transform.position.z 
            || DataPersistenceManager.gameData.playerHp != GameManager.instance.playerScript.hp || DataPersistenceManager.gameData.playerStamina != GameManager.instance.playerScript.stamina)
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
    public void credits()
    {
        SceneManager.LoadScene("credits");
        StartCoroutine(returnToTitleMenu());
    }

    IEnumerator returnToTitleMenu()
    {
        yield return new WaitForSeconds(8f);
        SceneManager.LoadScene("title menu");
    }

    //CLASS SELECTION
    public void warrior()
    {
        playerClass.MyClass = 1;
        if (!PhotonNetwork.IsConnected)
            newGame();
    }
    public void mage()
    {
        playerClass.MyClass = 2;
        if (!PhotonNetwork.IsConnected)
            newGame();

    }
    public void archer()
    {
        playerClass.MyClass = 3;
        if (!PhotonNetwork.IsConnected)
            newGame();
    }

    // Co-op features
    public void LoadMultiplayer() { GameManager.instance.charSelectionMenu(); SceneManager.LoadScene("Lobby"); }
}
