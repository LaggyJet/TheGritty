// Worked on by - Natalie Lubahn, Emily Underwood, Kheera, Jacob Irvin
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Diagnostics.Contracts;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    //main instance
    public static GameManager instance;

    [Header("------ UI ------")]

    //serialized fields
    [SerializeField] GameObject menuActive;
    [SerializeField] GameObject menuPause;
    [SerializeField] GameObject menuWin;
    [SerializeField] GameObject menuLose;
    [SerializeField] GameObject menuSettings;
    [SerializeField] TMP_Text enemyCountText;
    [SerializeField] GameObject charSelect;
    [SerializeField] GameObject newGameWarning;
    public bool hasRespawned = false; 

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
    public bool spawnedEnemies;

    //Calls "Awake" instead to run before the other Start methods
    void Awake()
    {
        instance = this;
        player = GameObject.FindWithTag("Player");
        if (player != null) {
            playerScript = player.GetComponent<PlayerController>();
            playerLocation = player.transform.position;
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
            else if (menuActive == null)
            {
                statePause();
                menuActive = menuPause;
                menuActive.SetActive(isPaused);
            }
            else if (menuActive == menuPause)
            {
                stateResume();
            }
        }
    }

    //Setter
    public void SetPlayer() {
        player = GameObject.FindWithTag("Player");
        playerScript = player.GetComponent<PlayerController>();
        playerLocation = player.transform.position;
    }

    //PAUSE METHODS
    public void statePause()
    {
        isPaused = !isPaused;
        Time.timeScale = 0;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.Confined;
    }
    public void stateResume()
    {
        isPaused = !isPaused;
        Time.timeScale = 1;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        menuActive.SetActive(isPaused);
        menuActive = null;
    }
    public void stateResumeGameLoads()
    {
        isPaused = !isPaused;
        Time.timeScale = 1;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        menuActive = null;
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
    }

    //WIN/LOSE METHODS
    public void updateEnemy(int amount)
    {
        enemyCount += amount;

        /// Converting our enemy count to string 
        enemyCountText.text = enemyCount.ToString("F0");
    }

    public void gameWon()
    {
        statePause();
        menuActive = menuWin;
        menuActive.SetActive(isPaused);
    }

    public void gameLost()
    {
        statePause();
        menuActive = menuLose;
        menuActive.SetActive(isPaused);
    }

    //RESPAWN METHODS
    public void respawnAfterLost()
    {
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
    }
    public void Warning4NewGame()
    {
        statePause();
        menuActive = newGameWarning;
        menuActive.SetActive(isPaused);
    }
}
