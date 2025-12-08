using UnityEngine;
using UnityEngine.UI;

public class ActionsUI : TransitionableUIBase
{
    // TODO: remove Singleton?
    public static ActionsUI Instance { get; private set; }

    [Header("UI")]
    [SerializeField] private Button m_PlayButton;
    [SerializeField] private Button m_BullshitButton;
    [SerializeField] private Outline m_Outline;
    [SerializeField] private InvalidPlayMessageUI m_InvalidPlayNotif;
    [SerializeField] private GameObject m_TurnNotification;
    private bool m_isPlayerOut;

    protected override void Awake()
    {
        base.Awake();

        if (Instance != this && Instance != null)
        {
            Destroy(Instance.gameObject);
        }
        Instance = this;


        m_PlayButton.onClick.AddListener(() =>
        {
            PokerHand playingPokerHand = HandSelectionUI.Instance.GetSelection();
            if (playingPokerHand != null)
            {
                // TODO: clients have the last played hand, playable hand check can be done client side?
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
        m_TurnNotification.gameObject.SetActive(m_PlayButton.enabled);
    }

    private void OnEnable()
    {
        TurnManager.Instance.OnNextPlayerTurn += TurnManager_NextPlayerTurn;
    }

    private void OnDisable()
    {
        TurnManager.Instance.OnNextPlayerTurn -= TurnManager_NextPlayerTurn;
    }

    private void Start()
    {
        GameManager.Instance.RegisterActionsUIObservers();
    }

    private void TurnManager_NextPlayerTurn(bool isPlayerTurn, bool wasPlayersTurnPreviously)
    {
        SetTurnActions(isPlayerTurn, wasPlayersTurnPreviously);
    }

    public void SetPlayerOut()
    {
        m_isPlayerOut = true;
    }

    public void SetPlayerIn()
    {
        m_isPlayerOut = false;
    }

    private void SetTurnActions(bool isPlayerTurn, bool wasPlayersTurnPreviously)
    {
        m_PlayButton.enabled = !m_isPlayerOut && isPlayerTurn;
        m_BullshitButton.enabled = !m_isPlayerOut && !GameManager.Instance.IsBeginningOfRound() && !wasPlayersTurnPreviously;
        m_Outline.enabled = m_PlayButton.enabled;
        m_TurnNotification.gameObject.SetActive(m_PlayButton.enabled);
    }
}
