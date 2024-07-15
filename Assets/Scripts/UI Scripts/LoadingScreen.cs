using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LoadingScreen : MonoBehaviour
{
    //variables
    [SerializeField] private ClassSelection playerClass;
    public GameObject loadingScreen;
    public Image loadingBarFill;

    //new game methods - assigns the correct class, starts the coroutine, initializes the new game data, and resumes the game so the player doesn't start paused
    public void loadSceneWARRIOR(int sceneId) //warrior selected
    {
        GameManager.instance.classSelector.MyClass = 1;
        if (!GameManager.selectedMultiplayer) {
            GameManager.enemyCount = 0;
            StartCoroutine(loadSceneAsync(sceneId));
            DataPersistenceManager.Instance.NewGame();
            GameManager.instance.stateResumeGameLoads();
        }
        else
            SceneManager.LoadScene("Lobby");
    }
    public void loadSceneMAGE(int sceneId) //mage selected
    {
        GameManager.instance.classSelector.MyClass = 2;
        if (!GameManager.selectedMultiplayer) {
            GameManager.enemyCount = 0;
            StartCoroutine(loadSceneAsync(sceneId));
            DataPersistenceManager.Instance.NewGame();
            GameManager.instance.stateResumeGameLoads();
        }
        else
            SceneManager.LoadScene("Lobby");
    }
    public void loadSceneARCHER(int sceneId) //archer selected
    {
        GameManager.instance.classSelector.MyClass = 3;
        if (!GameManager.selectedMultiplayer) {
            GameManager.enemyCount = 0;
            StartCoroutine(loadSceneAsync(sceneId));
            DataPersistenceManager.Instance.NewGame();
            GameManager.instance.stateResumeGameLoads();
        }
        else
            SceneManager.LoadScene("Lobby");
    }

    //resume game method - starts the coroutine, loads the saved game data, and resumes the game so the player doesn't start paused
    public void loadSceneResume(int sceneId)
    {
        GameManager.enemyCount = 0;
        DataPersistenceManager.Instance.LoadGame();
        StartCoroutine(loadSceneAsync(sceneId));
        GameManager.instance.stateResumeGameLoads();
    }

    //loads title screen method - starts the coroutine, used when the game is quit to return to the title menu
    public void loadSceneTitle(int sceneId)
    {
        GameManager.enemyCount = 0;
        StartCoroutine(loadSceneAsync(sceneId));
    }

    //enumerator used for the coroutine loads the scene based on the passed id, and fills the load bar as the scene loads
    IEnumerator loadSceneAsync(int sceneId)
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneId);

        loadingScreen.SetActive(true);

        while (!operation.isDone)
        {
            float progressValue = Mathf.Clamp01(operation.progress/0.9f);
            loadingBarFill.fillAmount = progressValue;

            yield return null;
        }
    }
}
