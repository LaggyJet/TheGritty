// Worked on by - Natalie Lubahn, Emily Underwood, Kheera
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Diagnostics.Contracts;

public class GameManager : MonoBehaviour
{
    //main instance
    public static GameManager instance;

    //serialized fields
    [SerializeField] GameObject menuActive;
    [SerializeField] GameObject menuPause;
    [SerializeField] GameObject menuWin;
    [SerializeField] GameObject menuLose;
    [SerializeField] TMP_Text enemyCountText;
    [SerializeField] GameObject menuSettings;

    [SerializeField] int sfxStarting;
    [SerializeField] int bgmStarting;



    //public variables
    public Image playerHPBar;
    public bool isPaused;
    public GameObject player;
    public PlayerController playerScript;
    public Vector3 playerLocation;
    public GameObject oldActiveMenu;
    public GameObject settingsPublicVers;
    public GameObject menuActivePublicVers;
    public bool canJump;
    public ToggleFunctions toggleScript;
    public static int enemyCount = 0;

    //Calls "Awake" instead to run before the other Start methods
    void Awake()
    {
        instance = this;
        player = GameObject.FindWithTag("Player");
        playerScript = player.GetComponent<PlayerController>();
        playerLocation = player.transform.position;
        settingsPublicVers = menuSettings;

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("Cancel"))
        {
            if (menuActive == null)
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

    //SETTINGS METHODS
    public void openSettings()
    {
        oldActiveMenu = menuActive;
        isPaused = !isPaused;
        menuActive.SetActive(isPaused);
        menuActive = null;
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
        isPaused = !isPaused;
        menuActive = oldActiveMenu;
        menuActivePublicVers = oldActiveMenu;
        menuActive.SetActive(isPaused);
    }
    public void defaultSettings()
    {
        canJump = false;
        toggleScript.isOnToggle();
    }
}
