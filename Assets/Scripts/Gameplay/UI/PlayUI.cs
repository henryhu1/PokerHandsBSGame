using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayUI : TriggerUITransition
{
    [SerializeField] private NextRoundUI m_nextRoundUI;

    private void Awake()
    {
        m_nextRoundUI.Hide();

        PokerHandsBullshitGame.Instance.OnEndOfRound += GameManager_EndOfRound;
        PokerHandsBullshitGame.Instance.OnNextRoundStarting += GameManager_NextRoundStarting;
    }

    public void GameManager_NextRoundStarting()
    {
        DoTransition();
    }

    public void GameManager_EndOfRound(List<bool> _, List<PokerHand> __)
    {
        m_nextRoundUI.Show();
        DoTransition();
    }
}
