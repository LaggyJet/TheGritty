using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Pun;

public class storyText2 : MonoBehaviour
{
    [SerializeField] float secondsB4Start;
    public TextMeshProUGUI textMeshPro;
    [SerializeField] string text;
    float charactersPerSecond = 40;
    private bool activated = false;

    IEnumerator TypeTextUncapped(string line)
    {
        yield return new WaitForSeconds(secondsB4Start);
        float timer = 0;
        float interval = 1 / charactersPerSecond;
        string textBuffer = null;
        char[] chars = line.ToCharArray();
        int i = 0;

        while (i < chars.Length)
        {
            if (timer < Time.deltaTime)
            {
                textBuffer += chars[i];
                textMeshPro.text = textBuffer;
                timer += interval;
                i++;
            }
            else
            {
                timer -= Time.deltaTime;
                yield return null;
            }
        }

        yield return new WaitForSeconds(1f);

        textMeshPro.text = "";
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player" && !activated && !PhotonNetwork.InRoom)
        {
            StartCoroutine(TypeTextUncapped(text));
            activated = true;
        }
    }
}
