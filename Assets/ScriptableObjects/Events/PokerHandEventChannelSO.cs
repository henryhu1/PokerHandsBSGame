using System;
using UnityEngine;

[CreateAssetMenu(fileName = "PokerHandEventChannelSO", menuName = "Events/poker hand")]
public class PokerHandEventChannelSO : ScriptableObject
{
    public Action<PokerHand> OnEventRaised;

    public void RaiseEvent(PokerHand pokerHand)
    {
        OnEventRaised?.Invoke(pokerHand);
    }
}
