using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExistingHandsUI : MonoBehaviour
{
    [SerializeField] private ExistingHandItemUI m_ExistingHandItemUIPrefab;
    [SerializeField] private GameObject m_LogContent;

    private List<ExistingHandItemUI> m_ExistingHandItems;

    private void Awake()
    {
        m_ExistingHandItems = new List<ExistingHandItemUI>();
    }

    private void Start()
    {
        PokerHandsBullshitGame.Instance.OnEndOfRound += GameManager_EndOfRound;
        PokerHandsBullshitGame.Instance.OnNextRoundStarting += GameManager_NextRoundStarting;
        PokerHandsBullshitGame.Instance.OnRestartGame += GameManager_RestartGame;
    }

    private void GameManager_EndOfRound(List<bool> _, List<PokerHand> allHandsInPlay)
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

    private void ClearContent()
    {
        foreach (ExistingHandItemUI existingHandItem in m_ExistingHandItems) Destroy(existingHandItem.gameObject);
        m_ExistingHandItems.Clear();
    }

    private void GameManager_NextRoundStarting()
    {
        ClearContent();
    }

    private void GameManager_RestartGame()
    {
        ClearContent();
    }
}
