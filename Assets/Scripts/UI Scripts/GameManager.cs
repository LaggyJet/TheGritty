// Worked on by - Natalie Lubahn, Emily Underwood, Kheera, Jacob Irvin
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Diagnostics.Contracts;
using UnityEngine.SceneManagement;
using Unity.VisualScripting;
using Photon.Pun;
using UnityEngine.InputSystem;
using UnityEngine.Assertions;

public class GameManager : MonoBehaviour
{
    //main instance
    public static GameManager instance;

    [Header("------ UI ------")]

    //serialized fields
    [SerializeField] GameObject menuActive;
    [SerializeField] public GameObject menuPause;
    [SerializeField] GameObject menuWin;
    [SerializeField] GameObject menuLose;
    [SerializeField] GameObject menuSettings;
    [SerializeField] TMP_Text enemyCountText;
    [SerializeField] GameObject charSelect;
    [SerializeField] GameObject GameWarning;

    //public variables
    public Image playerHPBar;
    public Image staminaBar;
    public bool isPaused;
    public GameObject player;
    public PlayerController playerScript;
    public static Vector3 playerLocation;
    public static Vector3 playerSpawnLocation;
    public bool canJump;
    public static int enemyCount = 0;
    public GameObject oldActiveMenu;
    public GameObject settingsPublicVers;
    public GameObject menuActivePublicVers;
    public bool isShooting;
    public TextMeshProUGUI textMeshPro;
    public float displayTime;
    public bool hasRespawned = false;
    public static bool selectedMultiplayer = false;
    public ClassSelection classSelector;
    public GameObject skillTreeScreen;
    public bool isSkTrActive;

    [Header("------ Audio ------")]
    [SerializeField] AudioSource soundTrackAud;
    [SerializeField] AudioClip menuMusic;
    [SerializeField] public float menuVol;
    [SerializeField] AudioClip gamePlayMusic;
    [SerializeField] public float gamePlayVol;
    public bool isPlayingSTA = false; // Sound Track Audio



    //Calls "Awake" instead to run before the other Start methods
    void Awake()
    {
        instance = this;
        player = GameObject.FindWithTag("Player");
        classSelector = GameObject.FindWithTag("ClassSelector").GetComponent<ClassSelection>();
        isSkTrActive = false;
        if (player != null)
        {
            playerScript = player.GetComponent<PlayerController>();
            //playerLocation = player.transform.position;
            if (SceneManager.GetActiveScene().name == "title menu")
            {
                SoundTrackswitch(GameMusic.Menu); // TODO: this will need to change once we solidify awake screen
            }

        }
    }

    public void OnSkillTreeOpen(InputAction.CallbackContext ctxt) //skill tree
    {
        if (skillTreeScreen != null)
        {

            //checks if our input is called, and if the tree is active or not
            if (ctxt.performed && !isSkTrActive && SceneManager.GetActiveScene().name == "Build Scene")
            {
                skillTreeScreen.SetActive(true);
                statePause();
                isSkTrActive = !isSkTrActive;
            }
            else if (ctxt.performed && isSkTrActive && SceneManager.GetActiveScene().name == "Build Scene")
            {
                skillTreeScreen.SetActive(false);
                stateResumeGameLoads();
                isSkTrActive = !isSkTrActive;
                SoundTrackswitch(GameMusic.Gameplay);
               SkillTreeManager.Instance.SaveData(ref DataPersistenceManager.gameData);
                
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("Cancel"))
        {
            if (menuActive == menuSettings)
            {
                leaveSettings();
            }

            else if (SceneManager.GetActiveScene().name == "title menu")
            {
                #if UNITY_EDITOR
                    UnityEditor.EditorApplication.isPlaying = false;
                #else
                    Application.Quit();
                #endif
            }
            else if (skillTreeScreen.activeInHierarchy)
            {
                return;
            }
            else if (menuActive == null)
            {
                if (PhotonNetwork.IsMasterClient && PhotonNetwork.InRoom)
                    PhotonView.Get(this).RPC(nameof(SetEveryPlayerPauseState), RpcTarget.All);
                else if (!PhotonNetwork.IsConnected)
                    SetEveryPlayerPauseState();
            }
            else if (menuActive == menuPause)
            {
                if (PhotonNetwork.IsMasterClient && PhotonNetwork.InRoom)
                    PhotonView.Get(this).RPC(nameof(SetEveryPlayerUnpauseState), RpcTarget.All);
                else if (!PhotonNetwork.IsConnected)
                    stateResume();
            }
        }
    }

    [PunRPC]
    void SetEveryPlayerPauseState()
    {
        statePause();
        menuActive = menuPause;
        menuActive.SetActive(isPaused);
    }

    [PunRPC]
    void SetEveryPlayerUnpauseState()
    {
        stateResume();
    }

    //Setter
    public void SetPlayer()
    {
        player = GameObject.FindWithTag("Player");
        playerScript = player.GetComponent<PlayerController>();
        //playerLocation = player.transform.position;
    }
    //TEXT POP UPS
    public void ShowText(string message)
    {
        textMeshPro.text = message;
        textMeshPro.gameObject.SetActive(true);
        Invoke("HideText", displayTime);
    }

    void HideText()
    {
        textMeshPro.gameObject.SetActive(false);
    }

    //PAUSE METHODS
    public void statePause()
    {
        isPaused = !isPaused;
        Time.timeScale = 0;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.Confined;
        SoundTrackswitch(GameMusic.Menu);
    }
    public void stateResume()
    {
        isPaused = !isPaused;
        Time.timeScale = 1;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        menuActive.SetActive(isPaused);
        menuActive = null;
        SoundTrackswitch(GameMusic.Gameplay);
    }
    public void stateResumeGameLoads()
    {
        isPaused = !isPaused;
        Time.timeScale = 1;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        menuActive = null;
        //SoundTrackswitch(GameMusic.Gameplay);
    }

    //SETTINGS METHODS
    public void openSettings()
    {
        if (menuActive != null)
        {
            oldActiveMenu = menuActive;
            isPaused = !isPaused;
            menuActive.SetActive(isPaused);
            menuActive = null;
        }
        isPaused = !isPaused;
        menuActive = menuSettings;
        menuActivePublicVers = menuSettings;
        menuActive.SetActive(isPaused);
        SoundTrackswitch(GameMusic.Menu);
    }
    public void leaveSettings()
    {
        isPaused = !isPaused;
        menuActive.SetActive(isPaused);
        menuActive = null;
        if (oldActiveMenu != null)
        {
            isPaused = !isPaused;
            menuActive = oldActiveMenu;
            menuActivePublicVers = oldActiveMenu;
            menuActive.SetActive(isPaused);
        }
        SoundTrackswitch(GameMusic.Menu);
    }

    //WIN/LOSE METHODS
    public void updateEnemy(int amount)
    {
        enemyCount += amount;

        /// Converting our enemy count to string 
        enemyCountText.text = enemyCount.ToString("F0");
    }

    [PunRPC]
    public void gameWon()
    {
        statePause();
        menuActive = menuWin;

        if (PhotonNetwork.InRoom && !PhotonNetwork.IsMasterClient)
        {
            menuWin.transform.Find("Menu Title").GetComponent<TMP_Text>().text = "Waiting....";
            menuWin.transform.Find("Restart").gameObject.SetActive(false);
        }

        menuActive.SetActive(isPaused);
        SoundTrackswitch(GameMusic.Menu);
    }

    [PunRPC]
    public void gameLost()
    {
        statePause();
        menuActive = menuLose;

        if (PhotonNetwork.InRoom && !PhotonNetwork.IsMasterClient)
        {
            menuLose.transform.Find("Menu Title").GetComponent<TMP_Text>().text = "Waiting....";
            menuLose.transform.Find("Respawn").gameObject.SetActive(false);
            menuLose.transform.Find("Restart").gameObject.SetActive(false);
        }

        menuActive.SetActive(isPaused);
        SoundTrackswitch(GameMusic.Menu);
    }

    public void CallGameLost() { PhotonView.Get(this).RPC(nameof(gameLost), RpcTarget.All); }

    //RESPAWN METHODS
    [PunRPC]
    public void respawnAfterLost()
    {
        if (PhotonNetwork.InRoom && PhotonNetwork.IsMasterClient)
            PhotonView.Get(this).RPC(nameof(respawnAfterLost), RpcTarget.Others);

        if (menuActive == menuLose)
        {

            playerScript.controller.enabled = false;
            playerScript.Respawn();
            playerScript.controller.enabled = true;
        }
        stateResume();
    }

    //TITLE SCREEN METHODS
    public void charSelectionMenu()
    {
        if (menuActive != null)
        {
            isPaused = !isPaused;
            menuActive.SetActive(isPaused);
            menuActive = null;
        }
        isPaused = !isPaused;
        menuActive = charSelect;
        menuActive.SetActive(isPaused);
        SoundTrackswitch(GameMusic.Menu);
    }
    public void Warning4NewGame()
    {
        statePause();
        menuActive = GameWarning;
        menuActive.SetActive(isPaused);
        SoundTrackswitch(GameMusic.Menu);
    }
    public void Warning4SaveProgress()
    {
        oldActiveMenu = menuActive;
        isPaused = !isPaused;
        menuActive.SetActive(isPaused);
        menuActive = null;
        statePause();
        menuActive = GameWarning;
        menuActive.SetActive(isPaused);
        SoundTrackswitch(GameMusic.Menu);
    }

    // Switch between which sounds you'd like for any created scenes
    public enum GameMusic { Menu, Gameplay }

    public void SoundTrackswitch(GameMusic music)
    {

        // Check persistence with music 
        if (soundTrackAud.isPlaying && ((music == GameMusic.Menu && soundTrackAud.clip == menuMusic) || (music == GameMusic.Gameplay && soundTrackAud.clip == gamePlayMusic)))
        {
            return;
        }

        switch (music)
        {
            case GameMusic.Menu:
                if (soundTrackAud.clip != menuMusic)
                {
                    soundTrackAud.clip = menuMusic;
                    soundTrackAud.volume = menuVol;
                    soundTrackAud.Play();
                    isPlayingSTA = true;
                }
                break;

            case GameMusic.Gameplay:
                if (soundTrackAud.clip != gamePlayMusic)
                {
                    soundTrackAud.clip = gamePlayMusic;
                    soundTrackAud.volume = gamePlayVol;
                    soundTrackAud.Play();
                    isPlayingSTA = true;
                }

                break;
        }
    }
}