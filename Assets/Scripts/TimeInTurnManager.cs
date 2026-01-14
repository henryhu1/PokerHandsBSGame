using System.Collections;
using UnityEngine;

public class TimeInTurnManager : MonoBehaviour
{
    [Header("Firing Events")]
    [SerializeField] private VoidEventChannelSO OnTurnTimeout;

    private float timeInTurn;
    private Coroutine turnTimeCountdown;

    public void StartTurnCountdown()
    {
        StopTurnCountdown();
        turnTimeCountdown = StartCoroutine(StartTurnTimeCountdown());
    }

    public void StopTurnCountdown()
    {
        if (turnTimeCountdown != null)
        {
            StopCoroutine(turnTimeCountdown);
        }
    }

    private void ResetTimeInTurn()
    {
        var rules = GameSession.Instance.ActiveRules;
        TimeForTurnType timeForTurn = rules.timeForTurn;
        switch (timeForTurn)
        {
            case TimeForTurnType.Five:
                timeInTurn = 5;
                break;
            case TimeForTurnType.Ten:
                timeInTurn = 10;
                break;
            case TimeForTurnType.Fifteen:
                timeInTurn = 15;
                break;
        }
    }

    private IEnumerator StartTurnTimeCountdown(float bufferTime = 1)
    {
        var rules = GameSession.Instance.ActiveRules;
        TimeForTurnType timeForTurn = rules.timeForTurn;
        if (timeForTurn == TimeForTurnType.None) yield break;

        ResetTimeInTurn();
        timeInTurn += bufferTime;

        while (timeInTurn > 0)
        {
            timeInTurn -= Time.deltaTime;
            yield return null;
        }
        OnTurnTimeout.RaiseEvent();
    }
}
