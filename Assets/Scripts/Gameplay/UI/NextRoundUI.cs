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
    [SerializeField] private TextMeshProUGUI m_playerLeftText;

    [Header("Listening Events")]
    [SerializeField] private VoidEventChannelSO OnNextRoundStarting;
    [SerializeField] private VoidEventChannelSO OnRoundEnded;
    [SerializeField] private StringEventChannelSO OnPlayerLeft;
    [SerializeField] private VoidEventChannelSO OnGameWon;

    private TransitionableUIBase animatable;

    private const string k_readyText = "Ready";
    private const string k_unreadyText = "Unready";

    private bool m_canBeReady;
    private int m_totalPlayersToBeReady;

    private void Awake()
    {
        if (Instance != this && Instance != null)
        {
            Destroy(Instance.gameObject);
        }
        Instance = this;

        animatable = GetComponent<TransitionableUIBase>();

        m_canBeReady = GameManager.Instance.IsNotOut();
        ResetTotalPlayersText();
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
        OnNextRoundStarting.OnEventRaised += NextRoundStarting;
        OnGameWon.OnEventRaised += GameWon;
    }

    private void OnDisable()
    {
        OnRoundEnded.OnEventRaised -= EndOfRound;
        OnPlayerLeft.OnEventRaised -= PlayerLeft;
        OnNextRoundStarting.OnEventRaised -= NextRoundStarting;
        OnGameWon.OnEventRaised -= GameWon;
    }

    private void Start()
    {
        GameManager.Instance.RegisterNextRoundUIObservers();
    }

    private void EndOfRound()
    {
        ResetTotalPlayersText();
        m_playerLeftText.gameObject.SetActive(false);
        if (animatable.IsOffScreen())
        {
            m_toggleText.text = k_readyText;
            m_readyForNextRoundToggle.enabled = m_canBeReady;
            m_readyForNextRoundToggle.isOn = false;
            animatable.StartAnimation();
        }
    }

    private void PlayerLeft(string playerLeftName)
    {
        ResetTotalPlayersText();
        m_playerLeftText.gameObject.SetActive(true);
        m_playerLeftText.text = $"{playerLeftName} has left";
        if (animatable.IsOffScreen())
        {
            m_toggleText.text = k_readyText;
            m_readyForNextRoundToggle.enabled = m_canBeReady;
            m_readyForNextRoundToggle.isOn = false;
            animatable.StartAnimation();
        }
    }

    private void NextRoundStarting()
    {
        if (!animatable.IsOffScreen())
        {
            m_toggleText.text = k_readyText;
            m_readyForNextRoundToggle.enabled = false;
            m_readyForNextRoundToggle.isOn = false;
            animatable.StartAnimation();
        }
    }

    private void GameWon()
    {
        if (!animatable.IsOffScreen())
        {
            m_toggleText.text = k_readyText;
            m_readyForNextRoundToggle.enabled = false;
            m_readyForNextRoundToggle.isOn = false;
            animatable.StartAnimation();
        }
    }

    private void ResetTotalPlayersText()
    {
        m_totalPlayersToBeReady = GameManager.Instance.m_inPlayClientIds.Count;
        m_playersReadyText.text = $"0/{m_totalPlayersToBeReady}";
    }

    public void SetNumberOfPlayersReadyText(int numberOfPlayersReady)
    {
        m_playersReadyText.text = $"{numberOfPlayersReady}/{m_totalPlayersToBeReady}";
    }

    public void SetCanBeReady(bool canBeReady)
    {
        m_canBeReady = canBeReady;
    }
}
