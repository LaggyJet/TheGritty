//Worked on By : Joshua Furber
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using TMPro;
public class HandleLobbies : MonoBehaviourPunCallbacks {
    [SerializeField] TMP_InputField hostInput, joinInput;
    [SerializeField] GameObject loadMenu, lobbyMenu;

    // Start the connection when loading screen plays
    void Start() { PhotonNetwork.AutomaticallySyncScene = false; PhotonNetwork.ConnectUsingSettings(); }

    // Have the player join the lobby once it loads
    public override void OnConnectedToMaster() { PhotonNetwork.JoinLobby(TypedLobby.Default); }

    // Display the lobby menu
    public override void OnJoinedLobby() { loadMenu.SetActive(false); lobbyMenu.SetActive(true); }

    // Create room based on user input (max of 2 players)
    public void HostRoom() { PhotonNetwork.CreateRoom(hostInput.text, new RoomOptions() { MaxPlayers = 2 }, null); }

    // Join room based on user input
    public void JoinRoom() { PhotonNetwork.JoinRoom(joinInput.text, null); }

    // Once player loads, load the scene
    public override void OnJoinedRoom() { PhotonNetwork.LoadLevel("PlayScene"); }
}