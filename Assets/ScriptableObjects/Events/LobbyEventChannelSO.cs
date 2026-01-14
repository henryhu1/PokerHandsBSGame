using System;
using Unity.Services.Lobbies.Models;
using UnityEngine;

[CreateAssetMenu(fileName = "LobbyEventChannelSO", menuName = "Events/lobby")]
public class LobbyEventChannelSO : ScriptableObject
{
    public Action<Lobby> OnEventRaised;

    public void RaiseEvent(Lobby lobby)
    {
        OnEventRaised?.Invoke(lobby);
    }
}
