using System;
using UnityEngine;

[CreateAssetMenu(fileName = "VoidEventChannelSO", menuName = "Events/void")]
public class VoidEventChannelSO : ScriptableObject
{
    public Action OnEventRaised;

    public void RaiseEvent()
    {
        OnEventRaised?.Invoke();
    }
}
