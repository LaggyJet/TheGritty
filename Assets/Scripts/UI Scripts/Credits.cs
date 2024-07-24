using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Credits : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Time.timeScale = 1f;
        StartCoroutine(returnToTitleMenu());
    }

    private void Update()
    {
        if (Input.GetButtonDown("Cancel"))
        {
            LoadingScreen.instance.loadSceneTitle(0);
            //SceneManager.LoadScene("title menu");
        }
    }

    IEnumerator returnToTitleMenu()
    {
        yield return new WaitForSeconds(12.3f);
        SceneManager.LoadScene("title menu");
        Time.timeScale = 1f;
    }
}
