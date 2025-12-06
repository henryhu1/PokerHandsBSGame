using System;
using UnityEngine;

[CreateAssetMenu(fileName = "StringEventChannelSO", menuName = "Events/string")]
public class StringEventChannelSO : ScriptableObject
{
    public Action<string> OnEventRaised;

    public void RaiseEvent(string str)
    {
        OnEventRaised?.Invoke(str);
    }
}
