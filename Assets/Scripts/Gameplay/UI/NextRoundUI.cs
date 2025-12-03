using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NextRoundUI : TransitionableUIBase
{
    public static NextRoundUI Instance { get; private set; }

    [SerializeField] private Toggle m_readyForNextRoundToggle;
    [SerializeField] private TextMeshProUGUI m_playersReadyText;
    [SerializeField] private TextMeshProUGUI m_toggleText;
    [SerializeField] private TextMeshProUGUI m_playerLeftText;
    private const string k_readyText = "Ready";
    private const string k_unreadyText = "Unready";

    private bool m_canBeReady;
    private int m_totalPlayersToBeReady;

    protected override void Awake()
    {
        base.Awake();

        if (Instance != this && Instance != null)
        {
            Destroy(Instance.gameObject);
        }
        Instance = this;

        m_canBeReady = GameManager.Instance.IsNotOut();
        m_totalPlayersToBeReady = GameManager.Instance.m_inPlayClientIds.Count;
        m_playersReadyText.text = $"0/{m_totalPlayersToBeReady}";
        m_toggleText.text = k_readyText;

        m_readyForNextRoundToggle.onValueChanged.AddListener((bool isOn) =>
        {
            string toggleText = isOn ? k_unreadyText : k_readyText;
            m_toggleText.text = toggleText;
            if (m_readyForNextRoundToggle.enabled)
            {
                GameManager.Instance.ReadyForNextRoundServerRpc(isOn);
            }
        });
    }

    protected override void RegisterForEvents()
    {
        GameManager.Instance.RegisterNextRoundUIObservers();
        GameManager.Instance.OnEndOfRound += GameManager_EndOfRound;
        GameManager.Instance.OnPlayerLeft += GameManager_PlayerLeft;
        GameManager.Instance.OnNextRoundStarting += GameManager_NextRoundStarting;
        GameManager.Instance.OnGameWon += GameManager_GameWon;
    }

    private void UnregisterFromEvents()
    {
        GameManager.Instance.OnEndOfRound -= GameManager_EndOfRound;
        GameManager.Instance.OnPlayerLeft -= GameManager_PlayerLeft;
        GameManager.Instance.OnNextRoundStarting -= GameManager_NextRoundStarting;
        GameManager.Instance.OnGameWon -= GameManager_GameWon;
    }

    protected override void Start()
    {
        RegisterForEvents();
        base.Start();
    }

    private void OnDestroy() { UnregisterFromEvents(); }

    private void GameManager_EndOfRound(List<bool> _, List<PokerHand> __)
    {
        m_playerLeftText.gameObject.SetActive(false);
        if (transform.position != m_originalPosition)
        {
            m_toggleText.text = k_readyText;
            m_readyForNextRoundToggle.enabled = m_canBeReady;
            m_readyForNextRoundToggle.isOn = false;
            StartAnimation();
        }
    }

    private void GameManager_PlayerLeft(string playerLeftName, List<bool> _, List<PokerHand> __)
    {
        m_playerLeftText.gameObject.SetActive(true);
        m_playerLeftText.text = $"{playerLeftName} has left";
        if (transform.position != m_originalPosition)
        {
            m_toggleText.text = k_readyText;
            m_readyForNextRoundToggle.enabled = m_canBeReady;
            m_readyForNextRoundToggle.isOn = false;
            StartAnimation();
        }
    }

    private void GameManager_NextRoundStarting()
    {
        if (transform.position != m_offScreenPosition)
        {
            m_toggleText.text = k_readyText;
            m_readyForNextRoundToggle.enabled = false;
            m_readyForNextRoundToggle.isOn = false;
            StartAnimation();
        }
    }

    private void GameManager_GameWon(int _, List<PlayerData> __)
    {
        if (transform.position != m_offScreenPosition)
        {
            m_toggleText.text = k_readyText;
            m_readyForNextRoundToggle.enabled = false;
            m_readyForNextRoundToggle.isOn = false;
            StartAnimation();
        }
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

    public void SetCanBeReady(bool canBeReady)
    {
        m_canBeReady = canBeReady;
    }
}
