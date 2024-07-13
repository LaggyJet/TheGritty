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
        GameManager.enemyCount = 0;
        playerClass.MyClass = 1;
        StartCoroutine(loadSceneAsync(sceneId));
        DataPersistenceManager.Instance.NewGame();
        GameManager.instance.stateResumeGameLoads();
    }
    public void loadSceneMAGE(int sceneId) //mage selected
    {
        GameManager.enemyCount = 0;
        playerClass.MyClass = 2;
        StartCoroutine(loadSceneAsync(sceneId));
        DataPersistenceManager.Instance.NewGame();
        GameManager.instance.stateResumeGameLoads();
    }
    public void loadSceneARCHER(int sceneId) //archer selected
    {
        GameManager.enemyCount = 0;
        playerClass.MyClass = 3;
        StartCoroutine(loadSceneAsync(sceneId));
        DataPersistenceManager.Instance.NewGame();
        GameManager.instance.stateResumeGameLoads();
    }

    //resume game method - starts the coroutine, loads the saved game data, and resumes the game so the player doesn't start paused
    public void loadSceneResume(int sceneId)
    {
        GameManager.enemyCount = 0;
        StartCoroutine(loadSceneAsync(sceneId));
        DataPersistenceManager.Instance.LoadGame();
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
