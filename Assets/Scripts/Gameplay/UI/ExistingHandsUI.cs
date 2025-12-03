using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExistingHandsUI : TransitionableUIBase
{
    [SerializeField] private ExistingHandItemUI m_ExistingHandItemUIPrefab;
    [SerializeField] private GameObject m_LogContent;

    private List<ExistingHandItemUI> m_ExistingHandItems;

    protected override void Awake()
    {
        base.Awake();

        m_ExistingHandItems = new List<ExistingHandItemUI>();
    }

    protected override void RegisterForEvents()
    {
        GameManager.Instance.OnEndOfRound += GameManager_EndOfRound;
        GameManager.Instance.OnPlayerLeft += GameManager_PlayerLeft;
        GameManager.Instance.OnNextRoundStarting += GameManager_NextRoundStarting;
        GameManager.Instance.OnGameWon += GameManager_GameWon;
        GameManager.Instance.OnRestartGame += GameManager_RestartGame;
    }

    private void UnregisterFromEvents()
    {
        GameManager.Instance.OnEndOfRound -= GameManager_EndOfRound;
        GameManager.Instance.OnPlayerLeft -= GameManager_PlayerLeft;
        GameManager.Instance.OnNextRoundStarting -= GameManager_NextRoundStarting;
        GameManager.Instance.OnGameWon -= GameManager_GameWon;
        GameManager.Instance.OnRestartGame -= GameManager_RestartGame;
    }

    protected override void Start()
    {
        RegisterForEvents();
        base.Start();
    }

    private void OnDestroy()
    {
        UnregisterFromEvents();
    }

    private void DisplayAllHandsInPlay(List<PokerHand> allHandsInPlay)
    {
        for (int i = 0; i < allHandsInPlay.Count; i++)
        {
            ExistingHandItemUI existingHandItem = Instantiate(m_ExistingHandItemUIPrefab, m_LogContent.transform);
            PokerHand hand = allHandsInPlay[i];
            (string, int) playedHandInfo = PlayedHandLogUI.Instance.GetPlayerAndRoundOfPlayedHand(hand);
            existingHandItem.GiveExistingHandItem(hand, playedHandInfo.Item1, playedHandInfo.Item2 + 1);
            m_ExistingHandItems.Add(existingHandItem);
        }
    }

    private void GameManager_EndOfRound(List<bool> _, List<PokerHand> allHandsInPlay)
    {
        DisplayAllHandsInPlay(allHandsInPlay);
        StartAnimation();
    }

    private void GameManager_PlayerLeft(string _, List<bool> __, List<PokerHand> allHandsInPlay)
    {
        DisplayAllHandsInPlay(allHandsInPlay);
        StartAnimation();
    }

    private void GameManager_GameWon(int _, List<PlayerData> __)
    {
        StartAnimation();
    }

    private void ClearContent()
    {
        foreach (ExistingHandItemUI existingHandItem in m_ExistingHandItems) Destroy(existingHandItem.gameObject);
        m_ExistingHandItems.Clear();
    }

    private void GameManager_NextRoundStarting()
    {
        StartAnimation();
        ClearContent();
    }

    private void GameManager_RestartGame()
    {
        ClearContent();
    }
}
