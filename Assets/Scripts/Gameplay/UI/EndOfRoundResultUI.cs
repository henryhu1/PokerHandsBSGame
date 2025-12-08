using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EndOfRoundResultUI : TransitionableUIBase
{
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI m_endOfRoundText;
    [SerializeField] private Image m_panel;

    [Header("Listening Events")]
    [SerializeField] private VoidEventChannelSO OnNextRoundStarting;
    [SerializeField] private IntEventChannelSO OnEndOfRoundResult;
    [SerializeField] private VoidEventChannelSO OnGameWon;

    private static Color s_safeColor = new(0, 0.9f, 0, 0.8f);
    private static Color s_loserColor = new(0.9f, 0, 0, 0.8f);

    private static Dictionary<RoundResultTypes, string> s_roundResultMessages = new()
    {
        { RoundResultTypes.Safe, "Safe!!" },
        { RoundResultTypes.CorrectBS, "You got it right!!" },
        { RoundResultTypes.CalledOut, "Called out..." },
        { RoundResultTypes.WrongBS, "You got it wrong..." },
    };

    private static Dictionary<RoundResultTypes, Color> s_roundResultColors = new()
    {
        { RoundResultTypes.Safe, s_safeColor },
        { RoundResultTypes.CorrectBS, s_safeColor },
        { RoundResultTypes.CalledOut, s_loserColor },
        { RoundResultTypes.WrongBS, s_loserColor },
    };

    private void OnEnable()
    {
        OnEndOfRoundResult.OnEventRaised += EndOfRoundResult;
        OnNextRoundStarting.OnEventRaised += NextRoundStarting;
        OnGameWon.OnEventRaised += GameWon;
    }

    private void OnDisable()
    {
        OnEndOfRoundResult.OnEventRaised -= EndOfRoundResult;
        OnNextRoundStarting.OnEventRaised -= NextRoundStarting;
        OnGameWon.OnEventRaised -= GameWon;
    }

    private void EndOfRoundResult(int roundResultValue)
    {
        if (!IsOffScreen()) return;

        RoundResultTypes roundResult = (RoundResultTypes) roundResultValue;
        m_panel.color = s_roundResultColors[roundResult];
        m_endOfRoundText.text = s_roundResultMessages[roundResult];
        StartAnimation();
    }

    private void NextRoundStarting()
    {
        if (!IsOffScreen()) StartAnimation();
    }

    private void GameWon()
    {
        if (!IsOffScreen()) StartAnimation();
    }
}
