using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class TurnManager : NetworkBehaviour
{
    public static TurnManager Instance;

    private List<ulong> m_turnOrder;
    private int m_currentTurnPlayerIndex;
    private NetworkVariable<ulong> m_currentTurnPlayerId = new NetworkVariable<ulong>(0);

    [HideInInspector]
    public delegate void NextPlayerTurnDelegateHandler(bool isPlayerTurn, bool wasPlayerTurnPreviously = false);
    [HideInInspector]
    public event NextPlayerTurnDelegateHandler OnNextPlayerTurn;

    public void Awake()
    {
        Instance = this;

        m_turnOrder = new List<ulong>();
    }

    public override void OnNetworkSpawn()
    {
        m_currentTurnPlayerId.OnValueChanged += (oldValue, newValue) =>
        {
            OnNextPlayerTurn?.Invoke(NetworkManager.LocalClientId == newValue, NetworkManager.LocalClientId == oldValue);
        };

        if (IsServer)
        {
            m_currentTurnPlayerIndex = 0;
            m_currentTurnPlayerId.Value = NetworkManager.LocalClientId;

            SceneTransitionHandler.Instance.OnClientLoadedScene += SceneTransitionHandler_OnClientLoadedScene;
        }

        base.OnNetworkSpawn();
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        if (IsServer)
        {
            SceneTransitionHandler.Instance.OnClientLoadedScene -= SceneTransitionHandler_OnClientLoadedScene;
        }
    }

    public void SceneTransitionHandler_OnClientLoadedScene(ulong clientId)
    {
        if (IsServer)
        {
            m_turnOrder.Add(clientId);
        }
    }

    public void AddPlayerToTurnOrder(ulong clientId)
    {
        if (IsServer)
        {

        }
    }

    public int GetPlayerTurnPosition(ulong playerId)
    {
        if (IsServer)
        {
            return m_turnOrder.IndexOf(playerId);
        }
        return -1;
    }

    public void AdvanceTurn()
    {
        if (IsServer)
        {
            SetTurn((m_currentTurnPlayerIndex + 1) % m_turnOrder.Count);
        }
    }

    private void SetTurn(int turnIndex)
    {
        if (IsServer)
        {
            m_currentTurnPlayerIndex = turnIndex;
            m_currentTurnPlayerId.Value = m_turnOrder[turnIndex];
        }
    }

    public void EndOfRound(ulong losingPlayerId)
    {
        if (IsServer)
        {
            // TODO: right now, after the round ends m_currentTurnPlayer.OnValueChanged will invoke the OnNextPlayerTurn event, but EndOfRoundClientRpc
            //   will invoke the OnNextPlayerTurn event correctly by setting wasPlayersTurnPreviously always to false. Fix.
            m_currentTurnPlayerId.Value = losingPlayerId;
            m_currentTurnPlayerIndex = m_turnOrder.IndexOf(losingPlayerId);
            EndOfRoundClientRpc(losingPlayerId);
        }
    }

    [ClientRpc]
    public void EndOfRoundClientRpc(ulong losingPlayerId)
    {
        OnNextPlayerTurn?.Invoke(NetworkManager.LocalClientId == losingPlayerId);
    }
}
