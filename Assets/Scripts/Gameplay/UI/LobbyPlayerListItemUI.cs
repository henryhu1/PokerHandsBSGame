using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine.UI;

[DefaultExecutionOrder(1000)]
public class LobbyPlayerListItemUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI m_playerNameText;
    [SerializeField] private Button m_kickPlayerButton;

    private Player player;


    private void Awake()
    {
        m_kickPlayerButton.onClick.AddListener(KickPlayer);
    }

    public void SetKickPlayerButtonVisible(bool visible)
    {
        m_kickPlayerButton.gameObject.SetActive(visible);
    }

    public void UpdatePlayer(Player player)
    {
        this.player = player;
        m_playerNameText.text = player.Data[LobbyManager.KEY_PLAYER_NAME].Value;
    }

    private void KickPlayer()
    {
        if (player != null)
        {
            LobbyManager.Instance.KickPlayer(player.Id);
        }
    }


}
