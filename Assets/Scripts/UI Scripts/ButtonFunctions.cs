//worked on by - natalie lubahn
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonFunctions : MonoBehaviour
{
    public void resume()
    {
        GameManager.instance.stateResume();
    }
    public void restart()
    {
        GameManager.enemyCount = 0;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        PlayerController.spawnHp = 10;
        GameManager.instance.playerScript.updatePlayerUI();
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

    //CLASS SELECTION
    public void warrior()
    {
        newGame();
    }
    public void mage()
    {
        newGame();
    }
    public void archer()
    {
        newGame();
    }
}
