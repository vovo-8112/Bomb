using Photon.Pun;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LeaveRoomButton : MonoBehaviour {
  private Button _button;

  private void Start() {
    _button = GetComponent<Button>();
    _button.onClick.AddListener(LeaveRoomButtonClick);
  }

  private void Leave() {
    PhotonNetwork.LeaveRoom();
  }

  private void LeaveRoomButtonClick() {
    SceneManager.LoadScene(0);
    Leave();
  }
}