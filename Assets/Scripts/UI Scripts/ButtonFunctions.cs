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
        GameManager.instance.stateResume();
    }
    public void quit()
    {
    #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
    #else
        Application.Quit();
    #endif
    }
    public void respawn()
    {
        GameManager.instance.respawnAfterLost();
    }
    public void settings()
    {
        GameManager.instance.openSettings();
    }
    public void returnButton()
    {
        GameManager.instance.leaveSettings();
    }
    public void defaultButton()
    {
        GameManager.instance.defaultSettings();
    }
    public void jumpToggle()
    {
        GameManager.instance.canJump = !GameManager.instance.canJump;
    }    
    
    //FOR TITLE SCREEN
    public void newGame()
    {
        SceneManager.LoadScene("Map Scene");
        DataPersistenceManager.Instance.NewGame();
    }
    public void loadGame()
    {
        SceneManager.LoadScene("Map Scene");
        DataPersistenceManager.Instance.LoadGame();
    }
}
