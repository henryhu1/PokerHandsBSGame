using System;
using System.Collections.Generic;
using Unity.Services.Lobbies.Models;
using UnityEngine;

[CreateAssetMenu(fileName = "LobbyListEventChannelSO", menuName = "Events/lobby list")]
public class LobbyListEventChannelSO : ScriptableObject
{
    public Action<List<Lobby>> OnEventRaised;

    public void RaiseEvent(List<Lobby> lobbies)
    {
        OnEventRaised?.Invoke(lobbies);
    }
}
