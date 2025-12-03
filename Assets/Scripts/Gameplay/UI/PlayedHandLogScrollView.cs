using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayedHandLogScrollView : ResizableUIBase
{
    private void Start()
    {
        GameManager.Instance.OnEndOfRound += GameManager_EndOfRound;
        GameManager.Instance.OnPlayerLeft += GameManager_PlayerLeft;
        GameManager.Instance.OnNextRoundStarting += GameManager_NextRoundStarting;
        GameManager.Instance.OnGameWon += GameManager_GameWon;
    }

    private void OnDestroy()
    {
        GameManager.Instance.OnEndOfRound -= GameManager_EndOfRound;
        GameManager.Instance.OnPlayerLeft -= GameManager_PlayerLeft;
        GameManager.Instance.OnNextRoundStarting -= GameManager_NextRoundStarting;
        GameManager.Instance.OnGameWon -= GameManager_GameWon;
    }

    private void GameManager_EndOfRound(List<bool> _, List<PokerHand> __)
    {
        StartAnimation();
    }

    private void GameManager_PlayerLeft(string _, List<bool> __, List<PokerHand> ___)
    {
        StartAnimation();
    }

    private void GameManager_NextRoundStarting()
    {
        StartAnimation();
    }

    private void GameManager_GameWon(int _, List<PlayerData> __)
    {
        StartAnimation();
    }
}
