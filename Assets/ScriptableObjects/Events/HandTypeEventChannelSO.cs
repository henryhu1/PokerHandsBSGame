using System;
using UnityEngine;

[CreateAssetMenu(fileName = "HandTypeEventChannelSO", menuName = "Events/hand")]
public class HandTypeEventChannelSO : ScriptableObject
{
    public Action<HandType> OnEventRaised;

    public void RaiseEvent(HandType hand)
    {
        OnEventRaised?.Invoke(hand);
    }
}
