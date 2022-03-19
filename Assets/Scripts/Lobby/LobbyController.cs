using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

public class LobbyController : MonoBehaviourPunCallbacks {
  [SerializeField]
  private Button _createRoom;

  [SerializeField]
  private Button _joinGame;

  private void Start() {
    PhotonNetworkSetup();
    SubscribeButton();
  }

  public override void OnConnectedToMaster() {
    Debug.Log("Conect");
  }

  public override void OnJoinedRoom() {
    Debug.Log("JoinRoom");
    PhotonNetwork.LoadLevel("GameScene");
  }

  private void SubscribeButton() {
    _createRoom.onClick.AddListener(CreateRoom);
    _joinGame.onClick.AddListener(JoinGame);
  }

  private void CreateRoom() {
    PhotonNetwork.CreateRoom(null);
  }

  private void JoinGame() {
    PhotonNetwork.JoinRandomRoom();
  }

  private static void PhotonNetworkSetup() {
    PhotonNetwork.NickName = "Player" + Random.Range(1, 10);
    PhotonNetwork.AutomaticallySyncScene = true;
    PhotonNetwork.GameVersion = "1.0.0";
    PhotonNetwork.ConnectUsingSettings();
  }
}