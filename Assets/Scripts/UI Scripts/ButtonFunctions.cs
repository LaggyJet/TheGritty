//worked on by - natalie lubahn, Jacob Irvin, Joshua Furber
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
    
    [PunRPC]
    public void restart()
    {
        if (PhotonNetwork.InRoom && PhotonNetwork.IsMasterClient)
            PhotonView.Get(this).RPC(nameof(restart), RpcTarget.Others);

        //using previous player - scene needs to know where to put the player
        GameManager.enemyCount = 0;
        GameManager.instance.playerScript.hp = GameManager.instance.playerScript.hpBase;
        GameManager.instance.playerScript.stamina = GameManager.instance.playerScript.staminaBase;
        GameManager.instance.player.transform.position = GameManager.playerLocation;
        DataPersistenceManager.Instance.SaveGame();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        DataPersistenceManager.Instance.LoadGame();
        GameManager.instance.stateResumeGameLoads();
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
        //Disconnect player from the server (if possible)
        if (PhotonNetwork.IsMasterClient && PhotonNetwork.InRoom)
            CallQuitGame();
        else if (PhotonNetwork.IsConnected)
            StartCoroutine(DisconnectPhoton());
        else if (!PhotonNetwork.IsConnected)
            SceneManager.LoadScene("title menu");
    }

    [PunRPC]
    IEnumerator DisconnectPhoton() {
        PhotonNetwork.Disconnect();

        while (PhotonNetwork.IsConnected)
            yield return null;

        GameManager.selectedMultiplayer = false;
        SceneManager.LoadScene("title menu");
    }

    public void CallQuitGame() { PhotonView.Get(this).RPC(nameof(DisconnectPhoton), RpcTarget.All); }

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
    }

    // Co-op features
    public void LoadMultiplayer() { GameManager.selectedMultiplayer = true; GameManager.instance.charSelectionMenu(); }
}
