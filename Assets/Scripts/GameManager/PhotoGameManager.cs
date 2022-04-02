using System.Collections.Generic;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace GameManager
{
    public class PhotoGameManager : MonoBehaviourPunCallbacks
    {
        [SerializeField]
        private Button m_LeaveRoomButton;

        [SerializeField]
        private List<Transform> m_ListSpawnPosition;

        [SerializeField]
        private MapController m_MapController;

        private List<Player> m_Characters = new List<Player>();

        private void Start()
        {
            PhotonPeer.RegisterType(typeof(MapDate), 243, MapDate.Serialize, MapDate.DeSerialize);
            m_LeaveRoomButton.onClick.AddListener(OnLeftRoom);

            SpawnPlayers();
        }

        private void SpawnPlayers()
        {
            if (PhotonNetwork.CurrentRoom.PlayerCount == 1)
            {
                PhotonNetwork.Instantiate("MinimalGridCharacter", m_ListSpawnPosition[0].position, Quaternion.identity);
            }
            else
            {
                PhotonNetwork.Instantiate("MinimalGridCharacterSecond", m_ListSpawnPosition[1].position, Quaternion.identity);
            }
        }

        public override void OnPlayerEnteredRoom(Player newPlayer)
        {
            Debug.LogFormat("Player{0} entered room", newPlayer.NickName);
            AddPlayer(newPlayer);
        }

        public override void OnPlayerLeftRoom(Player player)
        {
            Debug.LogFormat("Player{0} entered room", player.NickName);
        }

        public override void OnLeftRoom()
        {
            SceneManager.LoadScene(0);
        }

        private void AddPlayer(Player character)
        {
            m_Characters.Add(character);
        }
    }
}