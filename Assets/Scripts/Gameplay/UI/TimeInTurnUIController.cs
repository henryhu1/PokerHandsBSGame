using System.Collections;
using TMPro;
using UnityEngine;

public class TineInTurnUIController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI timerText;

    [Header("Listening Events")]
    [SerializeField] private IntEventChannelSO OnTimeForTurnDecided;
    [SerializeField] private VoidEventChannelSO OnCardsDistributed;
    [SerializeField] private BoolEventChannelSO OnNextPlayerTurn;
    [SerializeField] private VoidEventChannelSO OnRoundEnded;

    private TimeForTurnType timeForTurn;

    private float localTimeCountdown;
    private Coroutine localTimer;

    private void OnEnable()
    {
        timerText.enabled = false;

        OnTimeForTurnDecided.OnEventRaised += TimeForTurnDecided;
        OnCardsDistributed.OnEventRaised += CardsDistributed;
        OnNextPlayerTurn.OnEventRaised += NextPlayerTurn;
        OnRoundEnded.OnEventRaised += RoundEnded;
    }

    private void OnDisable()
    {
        OnTimeForTurnDecided.OnEventRaised -= TimeForTurnDecided;
        OnCardsDistributed.OnEventRaised -= CardsDistributed;
        OnNextPlayerTurn.OnEventRaised -= NextPlayerTurn;
        OnRoundEnded.OnEventRaised -= RoundEnded;
    }

    private void TimeForTurnDecided(int timeForTurn)
    {
        this.timeForTurn = (TimeForTurnType)timeForTurn;
    }

    private void CardsDistributed()
    {
        if (localTimer != null)
        {
            StopCoroutine(localTimer);
        }

        localTimer = StartCoroutine(RunDownTurnTimer());
    }

    private void NextPlayerTurn(bool isPlayerTurn)
    {
        if (localTimer != null)
        {
            StopCoroutine(localTimer);
        }

        // if (isPlayerTurn)
        // {
        localTimer = StartCoroutine(RunDownTurnTimer());
        // }
    }

    private void RoundEnded()
    {
        if (localTimer != null)
        {
            StopCoroutine(localTimer);
        }
        timerText.enabled = false;
    }

    private IEnumerator RunDownTurnTimer()
    {
        switch (timeForTurn)
        {
            case TimeForTurnType.Five:
                localTimeCountdown = 5;
                break;
            case TimeForTurnType.Ten:
                localTimeCountdown = 10;
                break;
            case TimeForTurnType.Fifteen:
                localTimeCountdown = 15;
                break;
        }

        while (localTimeCountdown > 0)
        {
            int oldTime = Mathf.FloorToInt(localTimeCountdown);

            localTimeCountdown -= Time.deltaTime;
            if (Mathf.FloorToInt(localTimeCountdown) != oldTime)
            {
                timerText.text = oldTime.ToString();
            }

            timerText.enabled = localTimeCountdown < 5;

            yield return null;
        }
        timerText.enabled = false;
    }
}
