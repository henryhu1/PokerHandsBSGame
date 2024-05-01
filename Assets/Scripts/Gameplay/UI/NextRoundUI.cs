using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NextRoundUI : MonoBehaviour
{
    public static NextRoundUI Instance;

    [SerializeField] private Toggle m_readyForNextRoundButton;
    [SerializeField] private TextMeshProUGUI m_playersReadyText;
    [SerializeField] private TextMeshProUGUI m_toggleText;
    private const string k_readyText = "Ready";
    private const string k_unreadyText = "Unready";

    private int m_totalPlayers;

    private void Awake()
    {
        Instance = this;

        m_totalPlayers = PokerHandsBullshitGame.Instance.m_numberOfPlayers.Value;
        m_playersReadyText.text = "0/" + m_totalPlayers;
        m_toggleText.text = k_readyText;

        m_readyForNextRoundButton.onValueChanged.AddListener((bool isOn) =>
        {
            string toggleText = isOn ? k_unreadyText : k_readyText;
            m_toggleText.text = toggleText;
            if (m_readyForNextRoundButton.enabled)
            {
                PokerHandsBullshitGame.Instance.ReadyForNextRoundServerRpc(isOn);
            }
        });

        PokerHandsBullshitGame.Instance.OnNextRoundStarting += GameManager_NextRoundStarting;
    }

    private void GameManager_NextRoundStarting()
    {
        m_toggleText.text = k_readyText;
        m_readyForNextRoundButton.enabled = false;
        m_readyForNextRoundButton.isOn = false;
    }

    public void SetNumberOfPlayersReadyText(int numberOfPlayersReady)
    {
        m_playersReadyText.text = numberOfPlayersReady.ToString() + "/" + m_totalPlayers;
    }

    public void Show()
    {
        gameObject.SetActive(true);
        m_readyForNextRoundButton.enabled = true;
    }

    public void Hide()
    {
        m_readyForNextRoundButton.enabled = false;
        m_readyForNextRoundButton.isOn = false;
        gameObject.SetActive(false);
    }
}
