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

    public void Awake()
    {
        Instance = this;

        m_turnOrder = new List<ulong>();
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
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
    }

    public void AdvanceTurn()
    {
        if (IsServer)
        {
            SetTurn((m_currentTurnPlayerIndex + 1) % PokerHandsBullshitGame.Instance.NumberOfPlayers);
        }
    }

    private void SetTurn(int turnIndex)
    {
        if (IsServer)
        {
            m_currentTurnPlayerIndex = turnIndex;
            m_currentTurnPlayerId.Value = m_turnOrder[m_currentTurnPlayerIndex];
        }
    }

    private void SetTurn(ulong playerId)
    {
        if (IsServer)
        {
            // TODO: right now, after the round ends m_currentTurnPlayer.OnValueChanged will invoke the OnNextPlayerTurn event, but EndOfRoundClientRpc
            //   will invoke the OnNextPlayerTurn event correctly by setting wasPlayersTurnPreviously always to false. Fix.
            m_currentTurnPlayerId.Value = playerId;
            m_currentTurnPlayerIndex = m_turnOrder.IndexOf(playerId);
        }
    }

}
