using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PokerHandListEventChannelSO", menuName = "Events/poker hand list")]
public class PokerHandListEventChannelSO : ScriptableObject
{
    public Action<List<PokerHand>> OnEventRaised;

    public void RaiseEvent(List<PokerHand> pokerHands)
    {
        OnEventRaised?.Invoke(pokerHands);
    }
}
