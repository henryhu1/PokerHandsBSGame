using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class TurnManager : NetworkBehaviour
{
    public static TurnManager Instance { get; private set; }
    private class TurnObject
    {
        public ulong clientId { get; set; }
        public int position { get; set; }
        public bool inPlay { get; set; }
        public TurnObject next { get; set; }
        public TurnObject previous { get; set; }

        public TurnObject(ulong clientId, int position, bool inPlay, TurnObject next, TurnObject previous)
        {
            this.clientId = clientId;
            this.position = position;
            this.inPlay = inPlay;
            this.next = next;
            this.previous = previous;
        }
    }

    // private List<ulong> m_turnOrder;
    private TurnObject m_firstTurn;
    private TurnObject m_lastTurn;
    private TurnObject m_currentTurnObject;
    private Dictionary<ulong, TurnObject> m_playerTurns;
    // TODO: why keep this list? When the game restarts, reset all TurnObject.next and TurnObject.previous using this order
    private List<TurnObject> m_turnPositions;
    // private HashSet<int> m_outOfPlayTurnIndices;
    private NetworkVariable<ulong> m_currentTurnClientId = new NetworkVariable<ulong>(0);

    [HideInInspector]
    public delegate void NextPlayerTurnDelegateHandler(bool isPlayerTurn, bool wasPlayerTurnPreviously = false);
    [HideInInspector]
    public event NextPlayerTurnDelegateHandler OnNextPlayerTurn;

    public void Awake()
    {
        Instance = this;

        m_playerTurns = new Dictionary<ulong, TurnObject>();
        m_turnPositions = new List<TurnObject>();
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

    public void RegisterCardManagerCallbacks()
    {
        CardManager.Instance.OnPlayerOut += CardManager_PlayerOut;
    }

    public void UnregisterCardManagerCallbacks()
    {
        CardManager.Instance.OnPlayerOut -= CardManager_PlayerOut;
    }

    private void CardManager_PlayerOut(ulong losingClientId)
    {
        if (IsServer)
        {
            m_playerTurns[losingClientId].inPlay = false;
            m_playerTurns[losingClientId].previous.next = m_playerTurns[losingClientId].next;
            m_playerTurns[losingClientId].next.previous = m_playerTurns[losingClientId].previous;
        }
    }

    public void AddClientToTurnOrder(ulong clientId)
    {
        if (IsServer)
        {
            Debug.Log($"Adding client #{clientId} to turn order, position {m_playerTurns.Count}");
            TurnObject clientTurn = new TurnObject(clientId, m_playerTurns.Count, true, null, null);
            // TODO: should I worry about clients loading the scene before the host?
            //   This would mean the first player (host) gets put somewhere not at the beginning for positioning
            if (m_playerTurns.Count == 0)
            {
                m_firstTurn = clientTurn;
                m_lastTurn = clientTurn;
                m_currentTurnObject = clientTurn;
            }
            else
            {
                m_lastTurn.next = clientTurn;
                clientTurn.previous = m_lastTurn;
                clientTurn.next = m_firstTurn;
                m_lastTurn = clientTurn;
                m_firstTurn.previous = m_lastTurn;
            }
            m_playerTurns.Add(clientId, clientTurn);
            m_turnPositions.Add(clientTurn);
        }
    }

    public List<ulong> GetTurnOrderStartingAtClient(ulong clientId)
    {
        if (IsServer)
        {
            int clientTurnPosition = m_playerTurns[clientId].position;
            List<ulong> after = m_turnPositions.GetRange(clientTurnPosition, m_turnPositions.Count - clientTurnPosition).Select(i => i.clientId).ToList();
            List<ulong> before = m_turnPositions.GetRange(0, clientTurnPosition).Select(i => i.clientId).ToList();
            return after.Concat(before).ToList();
        }
        return null;
    }

    public void AdvanceTurn()
    {
        if (IsServer)
        {
            m_currentTurnObject = m_currentTurnObject.next;
            m_currentTurnClientId.Value = m_currentTurnObject.clientId;
        }
    }

    public void EndOfRound(ulong losingClientId)
    {
        if (IsServer)
        {
            // TODO: right now, after the round ends m_currentTurnClientId.OnValueChanged will invoke the OnNextPlayerTurn event, but
            //   NextRoundClientRpc will invoke the OnNextPlayerTurn event correctly by setting wasPlayersTurnPreviously always to false. Fix.
            m_currentTurnObject = m_playerTurns[losingClientId];
            if (!m_currentTurnObject.inPlay)
            {
                m_currentTurnObject = m_currentTurnObject.next;
            }
            m_currentTurnClientId.Value = m_currentTurnObject.clientId;
        }
    }

    [ClientRpc]
    public void NextRoundClientRpc()
    {
        OnNextPlayerTurn?.Invoke(NetworkManager.LocalClientId == m_currentTurnClientId.Value);
    }

    //[ClientRpc]
    //public void PlayingNextRoundClientRpc(ClientRpcParams clientRpcParams = default)
    //{
    //    OnNextPlayerTurn?.Invoke(true, NetworkManager.LocalClientId == m_currentTurnClientId.Value);
    //}

    //[ClientRpc]
    //public void NotPlayingNextRoundClientRpc(ClientRpcParams clientRpcParams = default)
    //{
    //    OnNextPlayerTurn?.Invoke(false, NetworkManager.LocalClientId == m_currentTurnClientId.Value);
    //}
}
