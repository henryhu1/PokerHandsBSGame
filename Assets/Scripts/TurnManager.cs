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
        public bool inPlay { get; set; }
        public TurnObject next { get; set; }
        public TurnObject previous { get; set; }

        public TurnObject(ulong clientId, bool inPlay, TurnObject next, TurnObject previous)
        {
            this.clientId = clientId;
            this.inPlay = inPlay;
            this.next = next;
            this.previous = previous;
        }
    }

    // private List<ulong> m_turnOrder;
    private TurnObject m_currentTurnObject;
    private Dictionary<ulong, TurnObject> m_playerTurns;
    // TODO: why keep this list? When the game restarts, reset all TurnObject.next and TurnObject.previous using this order
    private List<TurnObject> m_turnPositions;
    // private HashSet<int> m_outOfPlayTurnIndices;
    private NetworkVariable<ulong> m_currentTurnClientId = new NetworkVariable<ulong>(0);

    [HideInInspector]
    public delegate void TurnOrderDecidedDelegateHandler();
    [HideInInspector]
    public event TurnOrderDecidedDelegateHandler OnTurnOrderDecided;

    [Header("Firing Events")]
    [SerializeField] private BoolEventChannelSO OnNextPlayerTurn;

    [Header("Listening Events")]
    [SerializeField] private UlongEventChannelSO OnPlayerOut;
    [SerializeField] private UlongEventChannelSO OnClientLoadedScene;

    public void Awake()
    {
        if (Instance != this && Instance != null)
        {
            Destroy(Instance.gameObject);
        }
        Instance = this;

        m_playerTurns = new Dictionary<ulong, TurnObject>();
        m_turnPositions = new List<TurnObject>();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        m_currentTurnClientId.OnValueChanged += (oldValue, newValue) =>
        {
            OnNextPlayerTurn.RaiseEvent(NetworkManager.Singleton.LocalClientId == newValue);
            // wasPlayersTurnPreviously = !GameManager.Instance.IsBeginningOfRound() && NetworkManager.Singleton.LocalClientId == oldValue
        };

        if (IsServer)
        {
            m_currentTurnClientId.Value = NetworkManager.Singleton.LocalClientId;

            NetworkManager.OnClientDisconnectCallback += RemovePlayer;
        }
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        if (IsServer)
        {
            m_playerTurns.Clear();

            NetworkManager.OnClientDisconnectCallback -= RemovePlayer;
        }
    }

    private void OnEnable()
    {
        OnPlayerOut.OnEventRaised += CardManager_PlayerOut;
        OnClientLoadedScene.OnEventRaised += AddClientToTurnOrder;
    }

    private void OnDisable()
    {
        OnPlayerOut.OnEventRaised -= CardManager_PlayerOut;
        OnClientLoadedScene.OnEventRaised -= AddClientToTurnOrder;
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
#if UNITY_EDITOR
            Debug.Log($"Adding client #{clientId} to turn order, position {m_playerTurns.Count}");
#endif
            TurnObject clientTurn = new TurnObject(clientId, true, null, null);
            // TODO: should I worry about clients loading the scene before the host?
            //   This would mean the first player (host) gets put somewhere not at the beginning for positioning
            if (!m_playerTurns.ContainsKey(clientId))
            {
                if (m_playerTurns.Count == 0)
                {
                    m_currentTurnObject = clientTurn;
                }
                else
                {
                    clientTurn.previous = m_turnPositions.Last();
                    clientTurn.next = m_turnPositions.First();
                    m_turnPositions.Last().next = clientTurn;
                    m_turnPositions.First().previous = clientTurn;
                }
                m_playerTurns.Add(clientId, clientTurn);
                m_turnPositions.Add(clientTurn);

                if (m_playerTurns.Count == GameManager.Instance.NumberOfPlayers)
                {
                    OnTurnOrderDecided?.Invoke();
                }
            }
#if UNITY_EDITOR
            else
            {
                Debug.Log($"Client #{clientId} already in turn order... at position {m_turnPositions.FindIndex(i => i.clientId == clientId)}");
            }
#endif
        }
    }

    public void RemovePlayer(ulong clientId)
    {
        if (IsServer)
        {
            if (m_playerTurns.ContainsKey(clientId))
            {
                if (m_playerTurns[clientId].inPlay)
                {
                    m_playerTurns[clientId].previous.next = m_playerTurns[clientId].next;
                    m_playerTurns[clientId].next.previous = m_playerTurns[clientId].previous;
                    m_playerTurns[clientId].inPlay = false;
                }
                m_playerTurns.Remove(clientId);
                m_turnPositions.RemoveAll(i => i.clientId == clientId);
#if UNITY_EDITOR
                Debug.Log($"removed {clientId} from turns");
#endif
            }
        }
    }

    public List<ulong> GetTurnOrderStartingAtClient(ulong clientId)
    {
        if (IsServer)
        {
            int clientTurnPosition = m_turnPositions.FindIndex(i => i.clientId == clientId);
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

    public void NextRound(ulong losingClientId)
    {
        if (IsServer)
        {
            // TODO: right now, after the round ends m_currentTurnClientId.OnValueChanged will invoke the OnNextPlayerTurn event, but
            //   NextRoundClientRpc will invoke the OnNextPlayerTurn event correctly by setting wasPlayersTurnPreviously always to false. Fix.
            if (m_playerTurns.ContainsKey(losingClientId))
            {
                m_currentTurnObject = m_playerTurns[losingClientId];
            }
            if (!m_currentTurnObject.inPlay)
            {
                m_currentTurnObject = m_currentTurnObject.next;
            }
            m_currentTurnClientId.Value = m_currentTurnObject.clientId;
            NextRoundClientRpc();
        }
    }

    public void NewGamePlayerTurns(List<ulong> inPlayClientIds, ulong winnerClientId)
    {
        if (IsServer)
        {
            for (int i = 0; i < m_turnPositions.Count; i++)
            {
                m_turnPositions[i].inPlay = true;

                if (i == 0)
                {
                    m_turnPositions[i].previous = m_turnPositions.Last();
                    m_turnPositions[i].next = m_turnPositions[i + 1];
                }
                else if (i == m_turnPositions.Count - 1)
                {
                    m_turnPositions[i].next = m_turnPositions.First();
                    m_turnPositions[i].previous = m_turnPositions[i - 1];
                }
                else
                {
                    m_turnPositions[i].next = m_turnPositions[i + 1];
                    m_turnPositions[i].previous = m_turnPositions[i - 1];
                }
            }

            foreach (ulong inPlayClientId in inPlayClientIds)
            {
                if (!m_playerTurns.ContainsKey(inPlayClientId))
                {
                    AddClientToTurnOrder(inPlayClientId);
                }
            }

            m_currentTurnObject = m_playerTurns[winnerClientId];
            m_currentTurnClientId.Value = winnerClientId;
        }
    }

    [ClientRpc]
    public void NextRoundClientRpc()
    {
        OnNextPlayerTurn.RaiseEvent(NetworkManager.Singleton.LocalClientId == m_currentTurnClientId.Value);
    }
}
