using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Pun;

public class UiText : MonoBehaviour
{
    public TextMeshProUGUI textMeshPro;
    private bool activated = false;

    IEnumerator uiText()
    {
        if (!activated)
        {
            textMeshPro.enabled = true;
            yield return new WaitForSeconds(2f);
            textMeshPro.enabled = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player" && !activated && !PhotonNetwork.InRoom)
        {
            StartCoroutine(uiText());
            activated = true;
        }
    }
}
