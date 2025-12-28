using UnityEngine;
using UnityEngine.UI;

public class ActionsUI : MonoBehaviour
{
    // TODO: remove Singleton?
    public static ActionsUI Instance { get; private set; }

    [Header("Hand selection")]
    [SerializeField] private HandSelectionUI handSelectionUI;

    [Header("UI")]
    [SerializeField] private Button m_PlayButton;
    [SerializeField] private Button m_BullshitButton;
    [SerializeField] private Outline m_Outline;
    [SerializeField] private InvalidPlayMessageUI m_InvalidPlayNotif;
    [SerializeField] private GameObject m_TurnNotification;

    [Header("Firing Events")]
    [SerializeField] private IntEventChannelSO OnInvalidPlay;
    [SerializeField] private PokerHandEventChannelSO OnSendPokerHandToPlay;

    [Header("Listening Events")]
    [SerializeField] private BoolEventChannelSO OnNextPlayerTurn;

    private bool m_isPlayerOut;

    private void Awake()
    {
        if (Instance != this && Instance != null)
        {
            Destroy(Instance.gameObject);
        }
        Instance = this;

        m_PlayButton.onClick.AddListener(() =>
        {
            PokerHand playingPokerHand = handSelectionUI.GetSelection();
            if (playingPokerHand == null)
            {
                return;
            }
            else if (GameManager.Instance.IsHandLowerThanLastPlayed(playingPokerHand))
            {
                OnInvalidPlay.RaiseEvent((int)InvalidPlays.HandTooLow);
            }
            else
            {
                OnSendPokerHandToPlay.RaiseEvent(playingPokerHand);
                GameManager.Instance.TryPlayingHandServerRpc(playingPokerHand);
            }
        });

        m_BullshitButton.onClick.AddListener(() =>
        {
            GameManager.Instance.EvaluateLastPlayedHandServerRpc();
        });

        m_isPlayerOut = false;

        m_PlayButton.enabled = GameManager.Instance.IsHost; // TODO: flimsy how based on host the play button enables, change to actual turn logic?
        m_BullshitButton.enabled = !GameManager.Instance.IsBeginningOfRound();
        m_Outline.enabled = m_PlayButton.enabled;
        m_TurnNotification.SetActive(m_PlayButton.enabled);
    }

    private void OnEnable()
    {
        OnNextPlayerTurn.OnEventRaised += TurnManager_NextPlayerTurn;
    }

    private void OnDisable()
    {
        OnNextPlayerTurn.OnEventRaised -= TurnManager_NextPlayerTurn;
    }

    private void Start()
    {
        GameManager.Instance.RegisterActionsUIObservers();
    }

    private void TurnManager_NextPlayerTurn(bool isPlayerTurn)
    {
        SetTurnActions(isPlayerTurn);
    }

    public void SetPlayerOut()
    {
        m_isPlayerOut = true;
    }

    public void SetPlayerIn()
    {
        m_isPlayerOut = false;
    }

    private void SetTurnActions(bool isPlayerTurn)
    {
        bool wasPlayerTurn = m_PlayButton.enabled;

        m_PlayButton.enabled = !m_isPlayerOut && isPlayerTurn;
        m_BullshitButton.enabled = !m_isPlayerOut && !GameManager.Instance.IsBeginningOfRound() && !wasPlayerTurn;
        m_Outline.enabled = m_PlayButton.enabled;
        m_TurnNotification.SetActive(m_PlayButton.enabled);
    }
}
