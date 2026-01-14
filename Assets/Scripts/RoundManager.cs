using UnityEngine;

public class RoundManager : MonoBehaviour
{
    private int roundNumber;

    [Header("Firing Events")]
    [SerializeField] private VoidEventChannelSO OnNextRoundStarting;
    [SerializeField] private VoidEventChannelSO OnRoundEnded;
    [SerializeField] private IntEventChannelSO OnEndOfRoundResult;

    [Header("Listening Events")]
    [SerializeField] private VoidEventChannelSO OnInitializeNewGame;

    private void Start()
    {
        roundNumber = 0;
    } 

    private void OnEnable()
    {
        OnInitializeNewGame.OnEventRaised += InitializeNewGame;
    }

    private void OnDisable()
    {
        OnInitializeNewGame.OnEventRaised -= InitializeNewGame;
    }

    private void InitializeNewGame() { roundNumber = 0; }

    public void EndOfRound()
    {
        OnRoundEnded.RaiseEvent();
    }

    public void EndOfRoundResult(RoundResultTypes roundResult)
    {
        OnEndOfRoundResult.RaiseEvent((int)roundResult);
    }

    public void StartNextRound()
    {
        roundNumber++;
        OnNextRoundStarting.RaiseEvent();
    }
}
