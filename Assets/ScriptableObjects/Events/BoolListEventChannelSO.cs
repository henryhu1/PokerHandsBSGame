using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BoolListEventChannelSO", menuName = "Events/bool list")]
public class BoolListEventChannelSO : ScriptableObject
{
    public Action<List<bool>> OnEventRaised;

    public void RaiseEvent(List<bool> areElementsTrue)
    {
        OnEventRaised?.Invoke(areElementsTrue);
    }
}
