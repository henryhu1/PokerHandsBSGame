using UnityEngine;

public class PlayedHandLogScrollView : ResizableUIBase
{
    [Header("Listening Events")]
    [SerializeField] private VoidEventChannelSO OnNextRoundStarting;
    [SerializeField] private VoidEventChannelSO OnRoundEnded;
    [SerializeField] private VoidEventChannelSO OnGameWon;

    private void OnEnable()
    {
        OnRoundEnded.OnEventRaised += StartAnimation;
        OnNextRoundStarting.OnEventRaised += StartAnimation;
        OnGameWon.OnEventRaised += StartAnimation;
    }

    private void OnDisable()
    {
        OnRoundEnded.OnEventRaised -= StartAnimation;
        OnNextRoundStarting.OnEventRaised -= StartAnimation;
        OnGameWon.OnEventRaised -= StartAnimation;
    }
}
