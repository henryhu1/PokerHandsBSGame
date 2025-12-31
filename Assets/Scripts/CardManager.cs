using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Assertions;

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

    [Header("Firing Events")]
    [SerializeField] private UlongEventChannelSO OnPlayerOut;
    [SerializeField] private BoolEventChannelSO OnAreFlushesAllowed;
    [SerializeField] private VoidEventChannelSO OnCardsDistributed;
    [SerializeField] private PlayerCardInfoListEventChannelSO OnRevealAllCards;

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
        cardGameServerManager = new(deckManager);
    }

    private void OnEnable()
    {
        OnClientLoadedScene.OnEventRaised += ClientLoadedScene;
    }

    private void OnDisable()
    {
        OnClientLoadedScene.OnEventRaised -= ClientLoadedScene;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsServer)
        {
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

    private void ClientLoadedScene(ulong clientId)
    {
        if (IsServer)
        {
            cardGameServerManager.InitializePlayerEmptyHand(clientId);
        }
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

    public void ForcePlayerOut(ulong clientId)
    {
        if (!IsServer) return;

        cardGameServerManager.SetPlayerOut(clientId);
        OnPlayerOut.RaiseEvent(clientId);
    }

    public void RevealAllCards()
    {
       if (IsServer)
        {
            cardGameServerManager.RevealAllCards();
        }
    }

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
    public void DistributeCardInfoToPlayerClientRpc(
        Card[] clientsCards,
        PlayerHiddenCardInfo[] otherClientsInfo,
        ulong[] clientOrder,
        bool areFlushesAllowed,
        ClientRpcParams clientRpcParams = default
    )
    {
        Assert.AreNotEqual(clientOrder.Count(), 0);
        Assert.AreEqual(clientOrder[0], NetworkManager.Singleton.LocalClientId);
        Assert.AreEqual(otherClientsInfo.Count(), clientOrder.Count());

        OnCardsDistributed.RaiseEvent();

        OnAreFlushesAllowed.RaiseEvent(areFlushesAllowed);

        deckGameObject.SetActive(GameManager.Instance.m_inPlayClientIds.Contains(NetworkManager.Singleton.LocalClientId));
        m_myCards = clientsCards.ToList();
        playerCardsInHand.CreateCardObjects(m_myCards);

        // TODO: why not do this server-side? Giving client work to do
        List<PlayerHiddenCardInfo> opponentCardsHidden = CardInfoHelper<PlayerHiddenCardInfo>.OrderDataByID(
            NetworkManager.Singleton.LocalClientId,
            otherClientsInfo,
            otherClientsInfo.Select(clientInfo => clientInfo.clientId).ToArray(),
            clientOrder
        );
        if (clientOrder.Length > 1)
        {
            allOpponentCards.DisplayHiddenOpponentCards(opponentCardsHidden);
        }
    }

    [ClientRpc]
    public void RevealCardInfoToPlayerClientRpc(
        PlayerCardInfo[] allPlayerCards,
        ulong[] clientOrder,
        ClientRpcParams clientRpcParams = default
    )
    {
        Assert.AreNotEqual(clientOrder.Count(), 0);
        Assert.AreEqual(clientOrder[0], NetworkManager.Singleton.LocalClientId);
        Assert.AreEqual(allPlayerCards.Count(), clientOrder.Count());

        // TODO: why not do this server-side? Giving client work to do
        List<PlayerCardInfo> opponentCards = CardInfoHelper<PlayerCardInfo>.OrderDataByID(
            NetworkManager.Singleton.LocalClientId,
            allPlayerCards,
            allPlayerCards.Select(clientInfo => clientInfo.clientId).ToArray(),
            clientOrder
        );
        if (clientOrder.Length > 1)
        {
            allOpponentCards.DisplayOpponentCards(opponentCards);
            OnRevealAllCards.RaiseEvent(allPlayerCards.ToList());
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
