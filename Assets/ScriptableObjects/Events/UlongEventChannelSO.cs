using System;
using UnityEngine;

[CreateAssetMenu(fileName = "UlongEventChannelSO", menuName = "Events/ulong")]
public class UlongEventChannelSO : ScriptableObject
{
    public Action<ulong> OnEventRaised;

    public void RaiseEvent(ulong num)
    {
        OnEventRaised?.Invoke(num);
    }
}
