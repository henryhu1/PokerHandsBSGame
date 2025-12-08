using System.Collections.Generic;
using UnityEngine;

public class ExistingHandsUI : TransitionableUIBase
{
    [Header("UI")]
    [SerializeField] private PlayedHandLogUI playedHandLogUI;
    [SerializeField] private ExistingHandItemUI m_ExistingHandItemUIPrefab;
    [SerializeField] private GameObject m_LogContent;

    [Header("Listening Events")]
    [SerializeField] private VoidEventChannelSO OnNextRoundStarting;
    [SerializeField] private VoidEventChannelSO OnInitializeNewGame;
    [SerializeField] private PokerHandListEventChannelSO OnDisplayAllHandsInPlay;
    [SerializeField] private VoidEventChannelSO OnGameWon;

    private List<ExistingHandItemUI> m_ExistingHandItems;

    protected override void Awake()
    {
        base.Awake();

        m_ExistingHandItems = new List<ExistingHandItemUI>();
    }

    private void OnEnable()
    {
        OnNextRoundStarting.OnEventRaised += NextRoundStarting;
        OnGameWon.OnEventRaised += GameWon;
        OnInitializeNewGame.OnEventRaised += InitializeNewGame;
        OnDisplayAllHandsInPlay.OnEventRaised += DisplayAllHandsInPlay;
    }

    private void OnDisable()
    {
        OnNextRoundStarting.OnEventRaised -= NextRoundStarting;
        OnGameWon.OnEventRaised -= GameWon;
        OnInitializeNewGame.OnEventRaised -= InitializeNewGame;
        OnDisplayAllHandsInPlay.OnEventRaised -= DisplayAllHandsInPlay;
    }

    private void DisplayAllHandsInPlay(List<PokerHand> allHandsInPlay)
    {
        for (int i = 0; i < allHandsInPlay.Count; i++)
        {
            ExistingHandItemUI existingHandItem = Instantiate(m_ExistingHandItemUIPrefab, m_LogContent.transform);
            PokerHand hand = allHandsInPlay[i];
            (string, int) playedHandInfo = playedHandLogUI.GetPlayerAndRoundOfPlayedHand(hand);
            existingHandItem.GiveExistingHandItem(hand, playedHandInfo.Item1, playedHandInfo.Item2 + 1);
            m_ExistingHandItems.Add(existingHandItem);
        }
        StartAnimation();
    }

    private void GameWon()
    {
        StartAnimation();
    }

    private void ClearContent()
    {
        foreach (ExistingHandItemUI existingHandItem in m_ExistingHandItems) Destroy(existingHandItem.gameObject);
        m_ExistingHandItems.Clear();
    }

    private void NextRoundStarting()
    {
        StartAnimation();
        ClearContent();
    }

    private void InitializeNewGame()
    {
        ClearContent();
    }
}
