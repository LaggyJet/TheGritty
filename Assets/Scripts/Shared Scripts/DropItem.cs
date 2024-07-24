using UnityEngine;
using Photon.Pun;
using System.Collections;

public static class DropItem {
    public static void TryDropItem(float chance, GameObject item, float itemHeight, GameObject enemy) {
        if (Random.value < chance && item != null && enemy != null) {
            GameObject itemSpawned;
            Vector3 spawnPosition = enemy.transform.position;

            if (PhotonNetwork.InRoom)
                itemSpawned = PhotonNetwork.Instantiate("ItemDrops/" + item.name, spawnPosition, enemy.transform.rotation);
            else if (!PhotonNetwork.InRoom) {
                itemSpawned = Object.Instantiate(item, spawnPosition, enemy.transform.rotation);
                Vector3 floatingPOS = enemy.transform.position + new Vector3(0, itemHeight, 0);
                itemSpawned?.GetComponent<MonoBehaviour>().StartCoroutine(AppearFromGround(itemSpawned.transform, floatingPOS, 5f));
            }
            else
                return;
        }
    }

    public static IEnumerator AppearFromGround(Transform item, Vector3 endPos, float duration) {
        float elapsedTime = 0f;

        while (elapsedTime < duration) {
            float lerpValue = elapsedTime / duration;
            item.position = Vector3.Lerp(item.position, endPos, lerpValue);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        item.position = endPos;
    }
}
