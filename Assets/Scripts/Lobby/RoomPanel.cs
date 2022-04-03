using Lobby;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RoomPanel : MonoBehaviour
{
    [SerializeField]
    private Button m_Button;

    [SerializeField]
    private TMP_Text m_RoomName;

    private LobbyController m_LobbyController;
    private string m_NameRoom;

    public void SetUp(RoomInfo roomInfo, LobbyController lobbyController)
    {
        m_NameRoom = roomInfo.Name;
        m_LobbyController = lobbyController;

        m_RoomName.SetText(m_NameRoom);
        m_Button.onClick.AddListener(JoinRoom);
    }

    private void JoinRoom()
    {
        m_LobbyController.JoinRoom(m_NameRoom);
    }
}