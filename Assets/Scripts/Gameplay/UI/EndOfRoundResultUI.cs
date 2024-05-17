using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class EndOfRoundResultUI : TransitionableUIBase
{
    [SerializeField] private TextMeshProUGUI m_endOfRoundText;
    [SerializeField] private Image m_panel;

    private static Color s_safeColor = new Color(0, 0.9f, 0, 0.8f);
    private static Color s_loserColor = new Color(0.9f, 0, 0, 0.8f);

    private static Dictionary<RoundResultTypes, string> s_roundResultMessages = new Dictionary<RoundResultTypes, string>
    {
        { RoundResultTypes.Safe, "Safe!!" },
        { RoundResultTypes.CorrectBS, "You got it right!!" },
        { RoundResultTypes.CalledOut, "Called out..." },
        { RoundResultTypes.WrongBS, "You got it wrong..." },
    };

    private static Dictionary<RoundResultTypes, Color> s_roundResultColors = new Dictionary<RoundResultTypes, Color>
    {
        { RoundResultTypes.Safe, s_safeColor },
        { RoundResultTypes.CorrectBS, s_safeColor },
        { RoundResultTypes.CalledOut, s_loserColor },
        { RoundResultTypes.WrongBS, s_loserColor },
    };

    protected override void RegisterForEvents()
    {
        GameManager.Instance.OnEndOfRoundResult += GameManager_EndOfRoundResult;
        GameManager.Instance.OnNextRoundStarting += GameManager_NextRoundStarting;
    }

    private void UnregisterFromEvents()
    {
        GameManager.Instance.OnEndOfRoundResult -= GameManager_EndOfRoundResult;
        GameManager.Instance.OnNextRoundStarting -= GameManager_NextRoundStarting;
    }

    private void GameManager_EndOfRoundResult(RoundResultTypes roundResult)
    {
        m_panel.color = s_roundResultColors[roundResult];
        m_endOfRoundText.text = s_roundResultMessages[roundResult];
        StartAnimation();
    }

    private void GameManager_NextRoundStarting()
    {
        if (gameObject.activeInHierarchy) StartAnimation();
    }

    protected override void Awake()
    {
        base.Awake();
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
}
