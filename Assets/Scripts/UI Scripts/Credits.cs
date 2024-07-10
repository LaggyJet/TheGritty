using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Credits : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(returnToTitleMenu());
    }

    IEnumerator returnToTitleMenu()
    {
        yield return new WaitForSeconds(7.6f);
        SceneManager.LoadScene("title menu");
    }
}
