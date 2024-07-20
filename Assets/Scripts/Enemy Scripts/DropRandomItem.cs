using UnityEngine;
using Photon.Pun;
using System.Collections;

public static class DropRandomItem {
    public static void TryDropItem(float chance, GameObject[] items, GameObject enemy) {
        int index = Random.Range(0, items.Length);

        if (Random.value < chance && items[index] && enemy != null) {
            GameObject item;
            Vector3 spawnPosition = enemy.transform.position - new Vector3(0, 0.6f, 0);

            if (PhotonNetwork.InRoom)
                item = PhotonNetwork.Instantiate("ItemDrops/" + items[index].name, spawnPosition, enemy.transform.rotation);
            else if (!PhotonNetwork.IsConnected)
                item = Object.Instantiate(items[index], spawnPosition, enemy.transform.rotation);
            else
                return;

            item?.GetComponent<MonoBehaviour>().StartCoroutine(AppearFromGround(item.transform, enemy.transform.position, 5f));
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
