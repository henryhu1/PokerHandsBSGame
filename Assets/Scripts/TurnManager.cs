using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class TurnManager : NetworkBehaviour
{
    public static TurnManager Instance;

    private List<ulong> m_turnOrder;
    private int m_currentTurnClientIndex;
    private NetworkVariable<ulong> m_currentTurnClientId = new NetworkVariable<ulong>(0);

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
        base.OnNetworkSpawn();

        m_currentTurnClientId.OnValueChanged += (oldValue, newValue) =>
        {
            OnNextPlayerTurn?.Invoke(NetworkManager.LocalClientId == newValue, NetworkManager.LocalClientId == oldValue);
        };

        if (IsServer)
        {
            m_currentTurnClientIndex = 0;
            m_currentTurnClientId.Value = NetworkManager.LocalClientId;

            SceneTransitionHandler.Instance.OnClientLoadedScene += AddClientToTurnOrder;
        }
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        if (IsServer)
        {
            SceneTransitionHandler.Instance.OnClientLoadedScene -= AddClientToTurnOrder;
        }
    }
    
    public void AddClientToTurnOrder(ulong clientId)
    {
        if (IsServer)
        {
            Debug.Log($"Adding client #{clientId} to turn order, position {m_turnOrder.Count}");
            m_turnOrder.Add(clientId);
        }
    }

    public int GetClientTurnPosition(ulong clientId)
    {
        if (IsServer)
        {
            return m_turnOrder.IndexOf(clientId);
        }
        return -1;
    }

    public List<ulong> GetTurnOrderStartingAtClient(ulong clientId)
    {
        if (IsServer)
        {
            int clientTurnPosition = GetClientTurnPosition(clientId);
            List<ulong> after = m_turnOrder.GetRange(clientTurnPosition, m_turnOrder.Count - clientTurnPosition);
            List<ulong> before = m_turnOrder.GetRange(0, clientTurnPosition);
            return after.Concat(before).ToList();
        }
        return null;
    }

    public void AdvanceTurn()
    {
        if (IsServer)
        {
            SetTurn((m_currentTurnClientIndex + 1) % m_turnOrder.Count);
        }
    }

    private void SetTurn(int turnIndex)
    {
        if (IsServer)
        {
            m_currentTurnClientIndex = turnIndex;
            m_currentTurnClientId.Value = m_turnOrder[turnIndex];
        }
    }

    public void NextRound(ulong losingClientId)
    {
        if (IsServer)
        {
            // TODO: right now, after the round ends m_currentTurnClientId.OnValueChanged will invoke the OnNextPlayerTurn event, but EndOfRoundClientRpc
            //   will invoke the OnNextPlayerTurn event correctly by setting wasPlayersTurnPreviously always to false. Fix.
            m_currentTurnClientId.Value = losingClientId;
            m_currentTurnClientIndex = m_turnOrder.IndexOf(losingClientId);
            NextRoundClientRpc(losingClientId);
        }
    }

    [ClientRpc]
    public void NextRoundClientRpc(ulong losingClientId)
    {
        OnNextPlayerTurn?.Invoke(NetworkManager.LocalClientId == losingClientId);
    }
}
