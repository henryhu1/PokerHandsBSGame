using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NextRoundUI : MonoBehaviour
{
    public static NextRoundUI Instance { get; private set; }

    [SerializeField] private Toggle m_readyForNextRoundButton;
    [SerializeField] private TextMeshProUGUI m_playersReadyText;
    [SerializeField] private TextMeshProUGUI m_toggleText;
    private const string k_readyText = "Ready";
    private const string k_unreadyText = "Unready";

    private int m_totalPlayersToBeReady;

    private void Awake()
    {
        Instance = this;

        m_totalPlayersToBeReady = PokerHandsBullshitGame.Instance.m_inPlayClientIds.Count;
        m_playersReadyText.text = $"0/{m_totalPlayersToBeReady}";
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

        PokerHandsBullshitGame.Instance.RegisterNextRoundUIObservers();
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
        m_playersReadyText.text = $"{numberOfPlayersReady}/{m_totalPlayersToBeReady}";
    }

    public void SetTotalNumberOfPlayersToBeReady(int numberOfPlayersToBeReady)
    {
        m_totalPlayersToBeReady = numberOfPlayersToBeReady;
        m_playersReadyText.text = $"0/{m_totalPlayersToBeReady}";
    }

    public void SetToggleEnabled(bool enabled)
    {
        m_readyForNextRoundButton.enabled = enabled;
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
