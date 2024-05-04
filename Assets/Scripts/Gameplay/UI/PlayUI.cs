using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayUI : TriggerUITransition
{
    public static PlayUI Instance { get; private set; }
    [SerializeField] private NextRoundUI m_nextRoundUI;

    private void Awake()
    {
        Instance = this;

        m_nextRoundUI.Hide();

        // PokerHandsBullshitGame.Instance.OnGameWon += GameManager_GameWon;
    }

    private void Start()
    {
        PokerHandsBullshitGame.Instance.OnEndOfRound += GameManager_EndOfRound;
        PokerHandsBullshitGame.Instance.OnNextRoundStarting += GameManager_NextRoundStarting;
    }

    private void OnDisable()
    {
        PokerHandsBullshitGame.Instance.OnEndOfRound -= GameManager_EndOfRound;
        PokerHandsBullshitGame.Instance.OnNextRoundStarting -= GameManager_NextRoundStarting;
        // PokerHandsBullshitGame.Instance.OnGameWon -= GameManager_GameWon;
    }

    public void GameManager_NextRoundStarting()
    {
        if (PokerHandsBullshitGame.Instance.IsNotOut())
        {
            DoTransition();
        }
        else
        {
            Hide();
        }
    }

    public void GameManager_EndOfRound(List<bool> _, List<PokerHand> __)
    {
        m_nextRoundUI.Show();
        DoTransition();
    }

    public void GameManager_GameWon(ulong winnerClientId, string winnerName, List<PokerHandsBullshitGame.PlayerData> eliminationOrder)
    {
        DoTransition();
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
