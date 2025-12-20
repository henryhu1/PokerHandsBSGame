using System;
using UnityEngine;

[CreateAssetMenu(fileName = "RankEventChannelSO", menuName = "Events/rank")]
public class RankEventChannelSO : ScriptableObject
{
    public Action<Rank> OnEventRaised;

    public void RaiseEvent(Rank rank)
    {
        OnEventRaised?.Invoke(rank);
    }
}
