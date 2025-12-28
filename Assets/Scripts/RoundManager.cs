using UnityEngine;

public class RoundManager : MonoBehaviour
{
    public static RoundManager Instance;

    private bool isRoundOver;

    [Header("Firing Events")]
    [SerializeField] private VoidEventChannelSO OnNextRoundStarting;
    [SerializeField] private VoidEventChannelSO OnRoundEnded;
    [SerializeField] private IntEventChannelSO OnEndOfRoundResult;

    [Header("Listening Events")]
    [SerializeField] private VoidEventChannelSO OnInitializeNewGame;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(Instance);
        }
        Instance = this;
    }

    private void Start()
    {
        isRoundOver = false;
    } 

    private void OnEnable()
    {
        OnInitializeNewGame.OnEventRaised += InitializeNewGame;
    }

    private void OnDisable()
    {
        OnInitializeNewGame.OnEventRaised -= InitializeNewGame;
    }

    private void InitializeNewGame() { isRoundOver = false; }

    public bool GetIsRoundOver() { return isRoundOver; }

    public void EndOfRound()
    {
        isRoundOver = true;
        OnRoundEnded.RaiseEvent();
    }

    public void EndOfRoundResult(RoundResultTypes roundResult)
    {
        OnEndOfRoundResult.RaiseEvent((int)roundResult);
    }

    public void StartNextRound()
    {
        isRoundOver = false;
        OnNextRoundStarting.RaiseEvent();
    }
}
