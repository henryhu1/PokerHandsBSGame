using System;
using UnityEngine;

[CreateAssetMenu(fileName = "ulongEventChannelSO", menuName = "Events/ulong")]
public class UlongEventChannelSO : ScriptableObject
{
    public Action<ulong> OnEventRaised;

    public void RaiseEvent(ulong num)
    {
        OnEventRaised?.Invoke(num);
    }
}
