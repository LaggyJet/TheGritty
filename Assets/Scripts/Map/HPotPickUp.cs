//Made by Emily Underwood, Kheera King 
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class HPotPickUp : MonoBehaviourPunCallbacks
{
    public enum SelectPotion {stamina, health}  
    // Amount of hp/stamina the potion adds to the player
    [Range(0f, 10f)] public float Amount;  
    public SelectPotion type;
    [SerializeField] AudioSource Audio;  
    [SerializeField] AudioClip pickUp;
    [SerializeField] float pickUpVol;
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && (other.GetComponent<PhotonView>().IsMine || !PhotonNetwork.IsConnected))
        {
            // Sound for player picking up potion 
            Audio.clip = pickUp; 
            Audio.volume = pickUpVol;
            Audio.Play();
            
            // If stamina or health 
            switch(type)
            {
                case SelectPotion.health:
                GameManager.instance.playerScript.AddHP(Amount);
                break;

                case SelectPotion.stamina:
                GameManager.instance.playerScript.AddStamina(Amount);
                break;

                default:
                Debug.Log("Wine? :)");
                break;
            }

            // Clean up 
            if (PhotonNetwork.InRoom) {
                PhotonView view = PhotonView.Get(this);
                if (view) {
                    if (view.IsMine || PhotonNetwork.IsMasterClient)
                        PhotonNetwork.Destroy(gameObject);
                    else
                        photonView.RPC(nameof(MasterDestroy), RpcTarget.MasterClient, view.ViewID);
                }
                else
                    Destroy(gameObject);
            }
            else if (!PhotonNetwork.InRoom)
                Destroy(gameObject);
        }
    }

    [PunRPC]
    void MasterDestroy(int viewID) {
        PhotonView view = PhotonView.Find(viewID);
        if (view && PhotonNetwork.IsMasterClient)
            PhotonNetwork.Destroy(view.gameObject);
    }
}
