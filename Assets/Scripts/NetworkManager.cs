using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

public class NetworkManager : MonoBehaviour {

    [SerializeField]
    Text connectionText;
    [SerializeField]
    Transform[] spawnPoints;
    [SerializeField]
    Camera sceneCamera;

    [SerializeField]
    GameObject serverWindow;
    [SerializeField]
    InputField username;
    [SerializeField]
    InputField roomName;
    [SerializeField]
    InputField roomList;

    [SerializeField]
    InputField messageWindow;

    GameObject player;
    Queue<string> messages;
    const int messageCount = 6;
    PhotonView photonView;

    // Use this for initialization
    void Start() {
        photonView = GetComponent<PhotonView>();
        messages = new Queue<string>(messageCount);

        PhotonNetwork.logLevel = PhotonLogLevel.Full;
        PhotonNetwork.ConnectUsingSettings("1.0");

        StartCoroutine("UpdateConnectionString");
    }

    IEnumerator UpdateConnectionString() {
        while (true) {
            connectionText.text = PhotonNetwork.connectionStateDetailed.ToString();
            yield return null;
        }
    }

    void OnJoinedLobby() {
        serverWindow.SetActive(true);
    }

    void OnJoinedRoom() {
        StopCoroutine("UpdateConnectionString");
        connectionText.text = "";

        serverWindow.SetActive(false);

        StartSpawnProcess(0f);
    }

    void OnReceivedRoomListUpdate() {
        roomList.text = "";
        RoomInfo[] rooms = PhotonNetwork.GetRoomList();
        foreach(RoomInfo room in rooms) {
            roomList.text += room.name + "\n";
        }
    }

    public void JoinRoom() {
        PhotonNetwork.player.name = username.text;
        RoomOptions ro = new RoomOptions() {
            IsVisible = true,
            MaxPlayers = 20
        };
        PhotonNetwork.JoinOrCreateRoom(roomName.text, ro, TypedLobby.Default);
    }

    void StartSpawnProcess(float respawnTime) {
        sceneCamera.enabled = true;
        StartCoroutine("SpawnPlayer", respawnTime);
    }

    IEnumerator SpawnPlayer(float respwanTime) {
        yield return new WaitForSeconds(respwanTime);
        int index = Random.Range(0, spawnPoints.Length);
        player = PhotonNetwork.Instantiate("Player",
            spawnPoints[index].position,
            spawnPoints[index].rotation,
            0);
        player.GetComponent<PlayerNetworkMover>().SendNetworkMessage += AddMessage;
        player.GetComponent<PlayerNetworkMover>().RespawnMe += StartSpawnProcess;

        sceneCamera.enabled = false;

        AddMessage("Spawned player:" + PhotonNetwork.player.name);
    }

    void AddMessage(string message) {
        photonView.RPC("AddMessage_RPC", PhotonTargets.All, message);
    }

    [PunRPC]
    void AddMessage_RPC(string message) {
        messages.Enqueue(message);

        if (messages.Count > messageCount) {
            messages.Dequeue();
        }

        messageWindow.text = "";

        foreach(string m in messages) {
            messageWindow.text += m + "\n";
        }
    }
}