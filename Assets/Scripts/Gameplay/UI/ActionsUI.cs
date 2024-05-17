using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ActionsUI : TransitionableUIBase
{
    public static ActionsUI Instance { get; private set; }

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

    protected override void RegisterForEvents()
    {
        CameraRotationLookAtTarget.Instance.OnCameraInPosition += CameraRotationLookAtTarget_CameraInPosition;
        PlayUI.Instance.OnShowPlayUI += PlayUI_ShowPlayUI;
        TurnManager.Instance.OnNextPlayerTurn += TurnManager_NextPlayerTurn;
        GameManager.Instance.OnEndOfRound += GameManager_EndOfRound;
        GameManager.Instance.OnPlayerLeft += GameManager_PlayerLeft;
        GameManager.Instance.OnNextRoundStarting += GameManager_NextRoundStarting;
        GameManager.Instance.OnRestartGame += GameManager_RestartGame;
    }

    private void UnregisterFromEvents()
    {
        CameraRotationLookAtTarget.Instance.OnCameraInPosition -= CameraRotationLookAtTarget_CameraInPosition;
        PlayUI.Instance.OnShowPlayUI -= PlayUI_ShowPlayUI;
        TurnManager.Instance.OnNextPlayerTurn -= TurnManager_NextPlayerTurn;
        GameManager.Instance.OnEndOfRound -= GameManager_EndOfRound;
        GameManager.Instance.OnPlayerLeft -= GameManager_PlayerLeft;
        GameManager.Instance.OnNextRoundStarting -= GameManager_NextRoundStarting;
        GameManager.Instance.OnRestartGame -= GameManager_RestartGame;
    }

    protected override void Start()
    {
        RegisterForEvents();
        GameManager.Instance.RegisterActionsUIObservers();
        base.Start();
    }

    private void OnDestroy()
    {
        UnregisterFromEvents();
    }

    private void CameraRotationLookAtTarget_CameraInPosition()
    {
        if (GameManager.Instance.IsNotOut()) StartAnimation();
    }

    private void PlayUI_ShowPlayUI()
    {
        StartAnimation();
    }

    private void TurnManager_NextPlayerTurn(bool isPlayerTurn, bool wasPlayersTurnPreviously)
    {
        SetTurnActions(isPlayerTurn, wasPlayersTurnPreviously);
    }

    private void GameManager_EndOfRound(List<bool> _, List<PokerHand> __)
    {
        if (GameManager.Instance.IsNotOut()) { StartAnimation(); }
    }

    private void GameManager_PlayerLeft(string _, List<bool> __, List<PokerHand> ___)
    {
        if (GameManager.Instance.IsNotOut()) { StartAnimation(); }
    }

    private void GameManager_NextRoundStarting()
    {
        if (GameManager.Instance.IsNotOut()) StartAnimation();
    }
    private void GameManager_RestartGame()
    {
        gameObject.SetActive(true);
        StartAnimation();
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
