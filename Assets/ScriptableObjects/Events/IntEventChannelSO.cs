using System;
using UnityEngine;

[CreateAssetMenu(fileName = "IntEventChannelSO", menuName = "Events/int")]
public class IntEventChannelSO : ScriptableObject
{
    public Action<int> OnEventRaised;

    public void RaiseEvent(int num)
    {
        OnEventRaised?.Invoke(num);
    }
}
