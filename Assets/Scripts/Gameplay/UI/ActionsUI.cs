using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ActionsUI : TriggerUITransition
    // TODO: TriggerUITransition may not be necessary.
    //   Instead, give this class the invalid message
    //   game object and have this control when the 
    //   fade animation plays. Delete TriggerUITransition.
    //   Can also stop the fading from here whenever Play is clicked
{
    public static ActionsUI Instance;

    [SerializeField] private Button m_PlayButton;
    [SerializeField] private Button m_BullshitButton;

    private void Awake()
    {
        Instance = this;

        m_PlayButton.onClick.AddListener(() =>
        {
            PokerHand playingPokerHand = HandSelectionUI.Instance.GetSelection();
            if (playingPokerHand != null)
            {
                PokerHandsBullshitGame.Instance.TryPlayingHandServerRpc(playingPokerHand);
            }
        });

        m_BullshitButton.onClick.AddListener(() =>
        {
            PokerHandsBullshitGame.Instance.EvaluateLastPlayedHandServerRpc();
        });

        m_PlayButton.enabled = PokerHandsBullshitGame.Instance.IsHosting(); // TODO: flimsy how based on host the play button enables, change to actual turn logic?
        m_BullshitButton.enabled = !PokerHandsBullshitGame.Instance.IsBeginningOfRound();
    }

    private void Start()
    {
        PokerHandsBullshitGame.Instance.OnInvalidPlay += DoTransition;
        TurnManager.Instance.OnNextPlayerTurn += TurnManager_NextPlayerTurn;
    }

    private void OnEnable()
    {
        TurnManager.Instance.OnNextPlayerTurn += TurnManager_NextPlayerTurn;
    }

    private void OnDisable()
    {
        TurnManager.Instance.OnNextPlayerTurn -= TurnManager_NextPlayerTurn;
    }

    private void TurnManager_NextPlayerTurn(bool isPlayerTurn, bool wasPlayersTurnPreviously)
    {
        SetTurnActions(isPlayerTurn, wasPlayersTurnPreviously);
    }

    private void SetTurnActions(bool isPlayerTurn, bool wasPlayersTurnPreviously)
    {
        m_PlayButton.enabled = isPlayerTurn;
        m_BullshitButton.enabled = !PokerHandsBullshitGame.Instance.IsBeginningOfRound() && !wasPlayersTurnPreviously;
    }
}
