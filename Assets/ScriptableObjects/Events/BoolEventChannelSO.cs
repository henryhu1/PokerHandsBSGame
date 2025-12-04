using System;
using UnityEngine;

[CreateAssetMenu(fileName = "BoolEventChannelSO", menuName = "Events/bool")]
public class BoolEventChannelSO : ScriptableObject
{
    public Action<bool> OnEventRaised;

    public void RaiseEvent(bool isTrue)
    {
        OnEventRaised?.Invoke(isTrue);
    }
}
