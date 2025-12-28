using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerCardInfoListEventChannelSO", menuName = "Events/player card info list")]
public class PlayerCardInfoListEventChannelSO : ScriptableObject
{
    public Action<List<PlayerCardInfo>> OnEventRaised;

    public void RaiseEvent(List<PlayerCardInfo>  playerCards)
    {
        OnEventRaised?.Invoke(playerCards);
    }
}
