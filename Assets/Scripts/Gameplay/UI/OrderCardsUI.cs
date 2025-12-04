using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OrderCardsUI : RotatableUIBase
{
    public static OrderCardsUI Instance;

    [Header("UI")]
    [SerializeField] private Button m_orderButton;

    [Header("Firing Events")]
    [SerializeField] private VoidEventChannelSO OnOrderCards;

    private bool isPointingInAscendingDirection = true;

    protected override void Awake()
    {
        base.Awake();

        if (Instance != null && Instance != this)
        {
            Destroy(Instance.gameObject);
        }
        Instance = this;
    }

    private void Start()
    {
        GameManager.Instance.OnEndOfRound += GameManager_EndOfRound;
        GameManager.Instance.OnPlayerLeft += GameManager_PlayerLeft;
        GameManager.Instance.OnNextRoundStarting += GameManager_NextRoundStarting;
        GameManager.Instance.OnRestartGame += GameManager_RestartGame;

        gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        GameManager.Instance.OnEndOfRound -= GameManager_EndOfRound;
        GameManager.Instance.OnPlayerLeft -= GameManager_PlayerLeft;
        GameManager.Instance.OnNextRoundStarting -= GameManager_NextRoundStarting;
        GameManager.Instance.OnRestartGame -= GameManager_RestartGame;
    }

    private void OnEnable()
    {
        m_orderButton.onClick.AddListener(() =>
        {
            if (!isPointingInAscendingDirection || PlayerCardsInHandManager.Instance.cardSortState != CardSortState.UNSORTED)
            {
                StartAnimation();
                isPointingInAscendingDirection = PlayerCardsInHandManager.Instance.cardSortState != CardSortState.ASCENDING;
            }
            OnOrderCards.RaiseEvent();
        });
    }

    private void OnDisable()
    {
        m_orderButton.onClick.RemoveAllListeners();
    }

    private void GameManager_RestartGame()
    {
        Show();
    }

    private void GameManager_NextRoundStarting()
    {
        Show();
    }

    private void GameManager_PlayerLeft(string playerLeftName, List<bool> playedHandsPresent, List<PokerHand> allHandsInPlay)
    {
        Hide();
    }

    private void GameManager_EndOfRound(List<bool> _, List<PokerHand> __)
    {
        Hide();
    }

    public void Show()
    {
        transform.eulerAngles = m_originalRotation;
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
