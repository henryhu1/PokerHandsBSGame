using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class CardManager : NetworkBehaviour
{
    public static CardManager Instance { get; private set; }
    private DeckManager deckManager;
    private CardGameServerManager cardGameServerManager;
    
    [Header("Data")]
    [SerializeField] private CardRegistrySO cardRegistry;

    [Header("Objects")]
    [SerializeField] private PlayerCardsInHandManager playerCardsInHand;
    [SerializeField] private AllOpponentCards allOpponentCards;
    [SerializeField] private GameObject deckGameObject;
    private List<Card> m_myCards;

    [HideInInspector]
    public delegate void AreFlushesAllowedHandsDelegateHandler(bool flushesAllowed);
    [HideInInspector]
    public event AreFlushesAllowedHandsDelegateHandler OnAreFlushesAllowed;

    [Header("Firing Events")]
    [SerializeField] private UlongEventChannelSO OnPlayerOut;

    [Header("Listening Events")]
    [SerializeField] private UlongEventChannelSO OnClientLoadedScene;

    private void Awake()
    {
        if (Instance != this && Instance != null)
        {
            Destroy(Instance.gameObject);
        }
        Instance = this;

        deckManager = new();
        cardGameServerManager = new(deckManager, OnClientLoadedScene);
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsServer)
        {
            // SceneTransitionHandler.Instance.OnClientLoadedScene += SceneTransitionHandler_ClientLoadedScene;
            NetworkManager.OnClientDisconnectCallback += RemovePlayer;

            cardGameServerManager.RegisterServerEvents();
            cardGameServerManager.ConfigureFromGameSettings();
        }
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        if (IsServer)
        {
            // SceneTransitionHandler.Instance.OnClientLoadedScene -= SceneTransitionHandler_ClientLoadedScene;
            NetworkManager.OnClientDisconnectCallback -= RemovePlayer;

            cardGameServerManager.UnregisterServerEvents();
        }
    }

    public void SetAmountOfCardsFromGameSetting()
    {
        if (IsServer)
        {
            cardGameServerManager.ConfigureFromGameSettings();
        }
    }

    private void CreateOtherPlayersCardsGameObjects(Dictionary<ulong, PlayerCardInfo> otherClientsCardInfo, ulong[] clientOrder)
    {
        // TODO: why not do this server-side? Giving client work to do
        List<List<Card>> opponentCards = new List<List<Card>>();
        List<string> opponentNames = new List<string>();
        foreach (ulong clientId in clientOrder)
        {
            PlayerCardInfo otherClientCardInfo = otherClientsCardInfo[clientId];
            opponentCards.Add(otherClientCardInfo.cards);
            opponentNames.Add(otherClientCardInfo.playerName);
        }
        allOpponentCards.DisplayOpponentCards(opponentCards, opponentNames, clientOrder);
    }

    public void RemovePlayer(ulong clientId)
    {
        if (IsServer)
        {
            cardGameServerManager.RemovePlayer(clientId);
        }
    }

    public bool IsHandInPlay(PokerHand pokerHand)
    {
        if (IsServer)
        {
            return cardGameServerManager.IsHandInPlay(pokerHand);
        }
        else return false;
    }

    public List<PokerHand> GetAllHandsInPlay()
    {
        if (IsServer)
        {
            return cardGameServerManager.GetHandsInPlay();
        }
        return new List<PokerHand>();
    }

    public bool IsFlushAllowedToBePlayed()
    {
        if (IsServer)
        {
            return cardGameServerManager.GetTotalCardsInPlay() <= CardManagerConstants.FlushLimit;
        }
        return false;
    }

    public void ChangeClientCardAmount(ulong clientId)
    {
        if (!IsServer) return;

        bool isPlayerOut = cardGameServerManager.ChangeClientCardAmount(clientId);
        if (isPlayerOut)
        {
            OnPlayerOut.RaiseEvent(clientId);
        }
    }

    //public void EndOfRound(ulong losingClientId, int cardAmonutChange)
    //{
    //    if (IsServer)
    //    {
    //        ChangeClientCardAmount(losingClientId, cardAmonutChange);
    //    }
    //}

    public void NextRound()
    {
        if (!IsServer) return;

        DestroyCardGameObjectsClientRpc();
        cardGameServerManager.ClearAllHands();
        cardGameServerManager.ResetHandsInPlay();
        cardGameServerManager.DistributeCards();
    }

    public void NewGamePlayerCards(List<ulong> inPlayClientIds)
    {
        if (!IsServer) return;

        DestroyCardGameObjectsClientRpc();
        cardGameServerManager.ConfigureFromGameSettings();
        cardGameServerManager.ClearPlayers();
        foreach (ulong clientId in inPlayClientIds)
        {
            cardGameServerManager.InitializePlayerEmptyHand(clientId);
        }
        cardGameServerManager.ResetHandsInPlay();
        cardGameServerManager.DistributeCards();
    }

    [ClientRpc]
    public void SendCardInfoToPlayerClientRpc(
        Card[] clientsCards,
        ulong[] allClientIds,
        PlayerCardInfo[] otherClientsCards,
        ulong[] clientOrder,
        bool areFlushesAllowed,
        ClientRpcParams clientRpcParams = default
    )
    {
        OnAreFlushesAllowed?.Invoke(areFlushesAllowed);

        deckGameObject.SetActive(GameManager.Instance.m_inPlayClientIds.Contains(NetworkManager.Singleton.LocalClientId));
        m_myCards = clientsCards.ToList();
        playerCardsInHand.CreateCardObjects(m_myCards);
        // m_areCardsSorted = false;

        Dictionary<ulong, PlayerCardInfo> otherClientsCardInfo = new Dictionary<ulong, PlayerCardInfo>();
        for (int i = 0; i < otherClientsCards.Length; i++)
        {
            if (NetworkManager.Singleton.LocalClientId != allClientIds[i])
            {
                PlayerCardInfo opponentCardInfo = otherClientsCards[i]; // new PlayerCardInfo(null, allClientCardAmounts[i], allClientNames[i]);
                otherClientsCardInfo.Add(allClientIds[i], opponentCardInfo);
            }
        }
        if (clientOrder.Length > 1)
        {
            CreateOtherPlayersCardsGameObjects(otherClientsCardInfo, clientOrder.Skip(1).Take(clientOrder.Length - 1).ToArray());
        }
    }

    [ClientRpc]
    public void DestroyCardGameObjectsClientRpc()
    {
        m_myCards.Clear();
        playerCardsInHand.DestroyCardGameObjects();
        allOpponentCards.HideOpponentHands();
    }
}
