using System.Collections.Generic;
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
        private GameObject m_LoadingPanel;

        [SerializeField]
        private MapController m_MapController;

        private List<Player> m_Characters = new List<Player>();
        private object invoke;
        private GameObject player;

        private void Start()
        {
            m_LeaveRoomButton.onClick.AddListener(OnLeftRoom);

            SpawnPlayers();
        }

        private void SpawnBot()
        {
            m_LoadingPanel.SetActive(false);

            var bot = PhotonNetwork.Instantiate("AiMinimalGridCharacter", m_ListSpawnPosition[1].position, Quaternion.identity).GetComponent<AiBomber>();
            bot.Target = player;
        }

        private void SpawnPlayers()
        {
            if (PhotonNetwork.CurrentRoom.PlayerCount == 1)
            {
                player = PhotonNetwork.Instantiate("MinimalGridCharacter", m_ListSpawnPosition[0].position, Quaternion.identity);
                Invoke(nameof(SpawnBot), 6f);
                m_LoadingPanel.SetActive(true);
            }
            else
            {
                PhotonNetwork.Instantiate("MinimalGridCharacterSecond", m_ListSpawnPosition[1].position, Quaternion.identity);
                m_LoadingPanel.SetActive(false);
                CancelInvoke(nameof(SpawnBot));
            }
        }

        public override void OnPlayerEnteredRoom(Player newPlayer)
        {
            AddPlayer(newPlayer);

            if (PhotonNetwork.IsMasterClient)
            {
                m_MapController.SendSyncDate(newPlayer);
            }
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