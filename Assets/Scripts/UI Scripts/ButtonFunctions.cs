//worked on by - natalie lubahn, Jacob Irvin, Joshua Furber, Kheera 
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;

public class ButtonFunctions : MonoBehaviourPun
{
    [SerializeField] private ClassSelection playerClass;

    public void resume()
    {
        GameManager.instance.PlayButtonClick();
        GameManager.instance.stateResume();
    }
    
    public void restart()
    {
        GameManager.instance.PlayMenuSwitchClick();
        if (PhotonNetwork.IsMasterClient)
            photonView.RPC(nameof(TriggerRestart), RpcTarget.All);
        else if (!PhotonNetwork.InRoom)
            restart();
    }

    [PunRPC]
    void TriggerRestart() {
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
        GameManager.instance.PlayMenuSwitchClick();

        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }

    public void quitGame()
    {
        GameManager.instance.PlayButtonClick();

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

    public void CallQuitGame() { photonView.RPC(nameof(DisconnectPhoton), RpcTarget.All); }

    public void respawn()
    {
        GameManager.instance.PlayMenuSwitchClick();
        GameManager.instance.respawnAfterLost();
    }
    public void settings()
    {
        GameManager.instance.PlayMenuSwitchClick();
        GameManager.instance.openSettings();
    }
    public void quitSettings()
    {
        GameManager.instance.PlayButtonClick();
        GameManager.instance.leaveSettings();
        if(SettingsManager.instance != null)
            SettingsManager.instance.SavePrefs();
        ResolutionManager.instance.SavePrefs();
    }

    public void quitSaveWarning()
    {
        GameManager.instance.PlayButtonClick();
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
        GameManager.instance.PlayMenuSwitchClick();
        GameManager.instance.Warning4NewGame();
    }
    public void startNewGamePart2()
    {
        GameManager.instance.PlayMenuSwitchClick();
        GameManager.instance.charSelectionMenu();
    }
    public void credits()
    {
        GameManager.instance.PlayMenuSwitchClick();
        SceneManager.LoadScene("credits");
    }

    // Co-op features
    public void LoadMultiplayer() { GameManager.instance.PlayMenuSwitchClick(); GameManager.selectedMultiplayer = true; GameManager.instance.charSelectionMenu(); }
}
