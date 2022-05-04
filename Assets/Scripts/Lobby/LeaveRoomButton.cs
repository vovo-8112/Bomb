using Photon.Pun;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Lobby
{
    public class LeaveRoomButton : MonoBehaviour
    {
        private Button _button;

        private void Start()
        {
            _button = GetComponent<Button>();

            if (_button == null)
            {
                return;
            }

            _button.onClick.AddListener(LeaveRoomButtonClick);
        }

        private void Leave()
        {
            PhotonNetwork.LeaveRoom();
        }

        public void LeaveRoomButtonClick()
        {
            SceneManager.LoadScene(0);
            Leave();
        }
    }
}