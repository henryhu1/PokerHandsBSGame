using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class TurnManager : NetworkBehaviour
{
    public static TurnManager Instance { get; private set; }

    [SerializeField] private TimeInTurnManager timeInTurnManager;

    private TurnObject currentTurnObject;
    private Dictionary<ulong, TurnObject> playerTurns;
    // TODO: why keep this list? When the game restarts, reset all TurnObject.next and TurnObject.previous using this order
    private List<TurnObject> turnPositions;

    [Header("Firing Events")]
    [SerializeField] private BoolEventChannelSO OnNextPlayerTurn;
    [SerializeField] private UlongEventChannelSO OnServerPlayerTurnTimeout;

    [Header("Listening Events")]
    [SerializeField] private VoidEventChannelSO OnTurnTimeout;
    [SerializeField] private UlongEventChannelSO OnPlayerOut;
    [SerializeField] private VoidEventChannelSO OnNextRoundStarting;
    [SerializeField] private VoidEventChannelSO OnRoundEnded;

    public void Awake()
    {
        if (Instance != this && Instance != null)
        {
            Destroy(Instance.gameObject);
        }
        Instance = this;

        playerTurns = new();
        turnPositions = new();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsServer)
        {
            NetworkManager.OnClientDisconnectCallback += RemovePlayer;
        }
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        if (IsServer)
        {
            playerTurns.Clear();
            turnPositions.Clear();

            NetworkManager.OnClientDisconnectCallback -= RemovePlayer;
        }
    }

    private void OnEnable()
    {
        OnTurnTimeout.OnEventRaised += TurnTimeout;
        OnNextRoundStarting.OnEventRaised += NextRoundStarting;
        OnRoundEnded.OnEventRaised += RoundEnded;
        OnPlayerOut.OnEventRaised += CardManager_PlayerOut;
    }

    private void OnDisable()
    {
        OnTurnTimeout.OnEventRaised -= TurnTimeout;
        OnNextRoundStarting.OnEventRaised -= NextRoundStarting;
        OnRoundEnded.OnEventRaised -= RoundEnded;
        OnPlayerOut.OnEventRaised -= CardManager_PlayerOut;
    }

    private void TurnTimeout()
    {
        if (!IsServer) return;

        OnServerPlayerTurnTimeout.RaiseEvent(currentTurnObject.ClientId);
    }

    private void NextRoundStarting()
    {
        timeInTurnManager.StartTurnCountdown();
    } 

    private void RoundEnded()
    {
        timeInTurnManager.StopTurnCountdown();
    }

    private void CardManager_PlayerOut(ulong losingClientId)
    {
        if (IsServer)
        {
            playerTurns[losingClientId].Previous.Next = playerTurns[losingClientId].Next;
            playerTurns[losingClientId].Next.Previous = playerTurns[losingClientId].Previous;
        }
    }

    // public void NewGamePlayerTurns(List<ulong> inPlayClientIds, ulong winnerClientId)
    // {
    //     if (IsServer)
    //     {
    //         for (int i = 0; i < turnPositions.Count; i++)
    //         {
    //             if (i == 0)
    //             {
    //                 turnPositions[i].Previous = turnPositions.Last();
    //                 turnPositions[i].Next = turnPositions[i + 1];
    //             }
    //             else if (i == turnPositions.Count - 1)
    //             {
    //                 turnPositions[i].Next = turnPositions.First();
    //                 turnPositions[i].Previous = turnPositions[i - 1];
    //             }
    //             else
    //             {
    //                 turnPositions[i].Next = turnPositions[i + 1];
    //                 turnPositions[i].Previous = turnPositions[i - 1];
    //             }
    //         }

    //         foreach (ulong inPlayClientId in inPlayClientIds)
    //         {
    //             if (!playerTurns.ContainsKey(inPlayClientId))
    //             {
    //                 AddClientToTurnOrder(inPlayClientId);
    //             }
    //         }

    //         currentTurnObject.Value = playerTurns[winnerClientId];

    //         timeInTurnManager.StartTurnCountdown(); // TODO: this needs to be done properly
    //     }
    // }

    public void DecideTurnOrder(ulong[] inPlayClientIds)
    {
        if (!IsServer) return;

        foreach (ulong clientId in inPlayClientIds)
        {
            AddClientToTurnOrder(clientId);
        }
    }

    private void AddClientToTurnOrder(ulong clientId)
    {
        if (IsServer)
        {
#if UNITY_EDITOR
            Debug.Log($"Adding client #{clientId} to turn order, position {playerTurns.Count}");
#endif
            TurnObject clientTurn = new(clientId, null, null);
            if (NetworkManager.Singleton.LocalClientId == clientId)
            {
                currentTurnObject = clientTurn;
            }

            if (!playerTurns.ContainsKey(clientId))
            {
                if (playerTurns.Count > 0)
                {
                    clientTurn.Previous = turnPositions.Last();
                    clientTurn.Next = turnPositions.First();
                    turnPositions.Last().Next = clientTurn;
                    turnPositions.First().Previous = clientTurn;
                }
                playerTurns.Add(clientId, clientTurn);
                turnPositions.Add(clientTurn);
            }
#if UNITY_EDITOR
            else
            {
                Debug.Log($"Client #{clientId} already in turn order... at position {turnPositions.FindIndex(i => i.ClientId == clientId)}");
            }
#endif
        }
    }

    private void RemovePlayer(ulong clientId)
    {
        if (IsServer)
        {
            if (playerTurns.ContainsKey(clientId))
            {
                playerTurns[clientId].Previous.Next = playerTurns[clientId].Next;
                playerTurns[clientId].Next.Previous = playerTurns[clientId].Previous;

                currentTurnObject = playerTurns[clientId];

                playerTurns.Remove(clientId);
                turnPositions.RemoveAll(i => i.ClientId == clientId);
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
            int clientTurnPosition = turnPositions.FindIndex(i => i.ClientId == clientId);
            List<ulong> after = turnPositions.GetRange(clientTurnPosition, turnPositions.Count - clientTurnPosition).Select(i => i.ClientId).ToList();
            List<ulong> before = turnPositions.GetRange(0, clientTurnPosition).Select(i => i.ClientId).ToList();
            return after.Concat(before).ToList();
        }
        return null;
    }

    public void AdvanceTurn()
    {
        if (IsServer)
        {
            // TODO: have currentTurnObject check if player is in game
            //   except when a player loses the doubly linked list is updated (next and prev)
#if UNITY_EDITOR
            Debug.Log($"client {currentTurnObject.ClientId} played, next up: {currentTurnObject.Next.ClientId}");
#endif
            currentTurnObject = currentTurnObject.Next;
            timeInTurnManager.StartTurnCountdown();
            NextTurnClientRpc(currentTurnObject.ClientId);
        }
    }

    public void ResetTurnForNewRound(ulong losingClientId)
    {
        if (IsServer)
        {
#if UNITY_EDITOR
            Debug.Log($"round restarting at client {losingClientId}, in play = {GameManager.Instance.IsClientInPlay(losingClientId)}, in turns = {playerTurns.ContainsKey(losingClientId)}");
#endif
            if (playerTurns.ContainsKey(losingClientId) && GameManager.Instance.IsClientInPlay(losingClientId))
            {
                currentTurnObject = playerTurns[losingClientId];
            }
            else
            {
                currentTurnObject = currentTurnObject.Next;
            }

            NextTurnClientRpc(currentTurnObject.ClientId);
        }
    }

    [ClientRpc]
    public void NextTurnClientRpc(ulong currentTurnClientId)
    {
        OnNextPlayerTurn.RaiseEvent(NetworkManager.Singleton.LocalClientId == currentTurnClientId);
    }
}
