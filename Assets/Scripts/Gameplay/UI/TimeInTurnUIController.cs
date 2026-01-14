using TMPro;
using UnityEngine;

public class TimeInTurnUIController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI timerText;

    [Header("Listening Events")]
    [SerializeField] private IntEventChannelSO OnSecondsLeft;
    [SerializeField] private VoidEventChannelSO OnNextRoundStarting;
    [SerializeField] private BoolEventChannelSO OnNextPlayerTurn;
    [SerializeField] private VoidEventChannelSO OnRoundEnded;

    private void OnEnable()
    {
        timerText.enabled = false;

        OnSecondsLeft.OnEventRaised += SecondsLeft;
        OnNextRoundStarting.OnEventRaised += NextRoundStarting;
        OnNextPlayerTurn.OnEventRaised += NextPlayerTurn;
        OnRoundEnded.OnEventRaised += RoundEnded;
    }

    private void OnDisable()
    {
        OnSecondsLeft.OnEventRaised -= SecondsLeft;
        OnNextRoundStarting.OnEventRaised -= NextRoundStarting;
        OnNextPlayerTurn.OnEventRaised -= NextPlayerTurn;
        OnRoundEnded.OnEventRaised -= RoundEnded;
    }

    private void SecondsLeft(int time)
    {
        timerText.text = time.ToString();
        timerText.enabled = true;
    }

    private void NextRoundStarting()
    {
        timerText.enabled = false;
    }

    private void NextPlayerTurn(bool isPlayerTurn)
    {
        timerText.enabled = false;
    }

    private void RoundEnded()
    {
        timerText.enabled = false;
    }
}
