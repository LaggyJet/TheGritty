// Worked on by - Joshua Furber
using UnityEngine;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;

public class AcidPuddle : MonoBehaviourPunCallbacks {
    [SerializeField] DamageStats stats_;
    [SerializeField] Renderer model;

    private void Start() { StartCoroutine(StartFade()); }

    IEnumerator StartFade() {
        yield return new WaitForSeconds(3f);
        var renderers = new List<Renderer>();
        Renderer[] childRenderers = transform.GetComponentsInChildren<Renderer>();
        renderers.AddRange(childRenderers);
        yield return new WaitForSeconds(3);
        while (model.material.color.a > 0) {
            foreach (Renderer render in renderers) {
                if (render.material.HasProperty("_Color"))
                {
                    RenderModeAdjuster.SetTransparent(render.material);
                    float fadeSpeed = render.material.color.a - Time.deltaTime;
                    render.material.color = new Color(render.material.color.r, render.material.color.g, render.material.color.b, fadeSpeed);
                }
                yield return null;
            }
        }

        if (PhotonNetwork.InRoom)
            PhotonNetwork.Destroy(gameObject);
        else if (!PhotonNetwork.InRoom)
            Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other) {
        IDamage damageCheck = other.GetComponent<IDamage>();
        if (damageCheck != null && other.CompareTag("Player"))
            damageCheck.Afflict(stats_);
    }
}
