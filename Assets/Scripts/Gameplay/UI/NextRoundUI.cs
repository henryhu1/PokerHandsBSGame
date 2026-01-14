using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NextRoundUI : MonoBehaviour
{
    // TODO: remove Singleton
    public static NextRoundUI Instance { get; private set; }

    [Header("UI")]
    [SerializeField] private Toggle m_readyForNextRoundToggle;
    [SerializeField] private TextMeshProUGUI m_playersReadyText;
    [SerializeField] private TextMeshProUGUI m_toggleText;
    [SerializeField] private GameObject offendingPlayerUI;
    [SerializeField] private TextMeshProUGUI offendingPlayerText;

    [Header("Listening Events")]
    [SerializeField] private VoidEventChannelSO OnNextRoundStarting;
    [SerializeField] private VoidEventChannelSO OnRoundEnded;
    [SerializeField] private StringEventChannelSO OnPlayerLeft;
    [SerializeField] private StringEventChannelSO OnPlayerRanOutOfTime;
    [SerializeField] private VoidEventChannelSO OnGameWon;

    private TransitionableUIBase animatable;

    private const string k_readyText = "Ready";
    private const string k_unreadyText = "Unready";

    private int m_totalPlayersToBeReady;

    private void Awake()
    {
        if (Instance != this && Instance != null)
        {
            Destroy(Instance.gameObject);
        }
        Instance = this;

        animatable = GetComponent<TransitionableUIBase>();

        offendingPlayerUI.SetActive(false);
        m_toggleText.text = k_readyText;

        m_readyForNextRoundToggle.onValueChanged.AddListener(isOn =>
        {
            string toggleText = isOn ? k_unreadyText : k_readyText;
            m_toggleText.text = toggleText;
            if (m_readyForNextRoundToggle.enabled)
            {
                GameManager.Instance.ReadyForNextRoundServerRpc(isOn);
            }
        });
    }

    private void OnEnable()
    {
        OnRoundEnded.OnEventRaised += EndOfRound;
        OnPlayerLeft.OnEventRaised += PlayerLeft;
        OnPlayerRanOutOfTime.OnEventRaised += PlayerRanOutOfTime;
        OnNextRoundStarting.OnEventRaised += NextRoundStarting;
        OnGameWon.OnEventRaised += GameWon;
    }

    private void OnDisable()
    {
        OnRoundEnded.OnEventRaised -= EndOfRound;
        OnPlayerLeft.OnEventRaised -= PlayerLeft;
        OnPlayerRanOutOfTime.OnEventRaised -= PlayerRanOutOfTime;
        OnNextRoundStarting.OnEventRaised -= NextRoundStarting;
        OnGameWon.OnEventRaised -= GameWon;
    }

    private void EndOfRound()
    {
        ResetTotalPlayersText();
        offendingPlayerUI.SetActive(false);
        // if (animatable.IsOffScreen())
        // {
        m_toggleText.text = k_readyText;
        m_readyForNextRoundToggle.enabled = GameManager.Instance.IsClientInPlay();
        m_readyForNextRoundToggle.isOn = false;
        animatable.TransitionOnToScreen();
        // }
    }

    private void PlayerLeft(string playerLeftName)
    {
        ResetTotalPlayersText();
        offendingPlayerUI.SetActive(true);
        offendingPlayerText.text = $"{playerLeftName} has left";
    }

    private void PlayerRanOutOfTime(string playerRanOutOfTimeName)
    {
        ResetTotalPlayersText();
        offendingPlayerUI.SetActive(true);
        offendingPlayerText.text = $"{playerRanOutOfTimeName} has timed out";
    }

    private void NextRoundStarting()
    {
        // if (!animatable.IsOffScreen())
        // {
        offendingPlayerUI.SetActive(false);
        m_toggleText.text = k_readyText;
        m_readyForNextRoundToggle.enabled = false;
        m_readyForNextRoundToggle.isOn = false;
        animatable.TransitionOffScreen();
        // }
    }

    private void GameWon()
    {
        // if (!animatable.IsOffScreen())
        // {
        offendingPlayerUI.SetActive(false);
        m_toggleText.text = k_readyText;
        m_readyForNextRoundToggle.enabled = false;
        m_readyForNextRoundToggle.isOn = false;
        animatable.TransitionOffScreen();
        // }
    }

    private void ResetTotalPlayersText()
    {
        m_totalPlayersToBeReady = GameManager.Instance.GetNumberOfInGamePlayers();
        m_playersReadyText.text = $"0/{m_totalPlayersToBeReady}";
    }

    public void SetTotalPlayersToBeReady(int totalPlayers)
    {
        m_totalPlayersToBeReady = totalPlayers;
    }

    public void SetNumberOfPlayersReadyText(int numberOfPlayersReady)
    {
        m_playersReadyText.text = $"{numberOfPlayersReady}/{m_totalPlayersToBeReady}";
    }
}
