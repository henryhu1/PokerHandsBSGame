using CardTraitExtensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using WebSocketSharp;

public class CardManager : NetworkBehaviour
{
    public static CardManager Instance { get; private set; }

    public struct PlayerCardInfo : INetworkSerializable
    {
        public List<Card> cards;
        public int amountOfCards;
        public string playerName;

        public PlayerCardInfo(List<Card> cards, int amountOfCards, string playerName)
        {
            this.cards = cards;
            this.amountOfCards = amountOfCards;
            this.playerName = playerName;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref amountOfCards);
            serializer.SerializeValue(ref playerName);
        }
    }

    public const int k_AscendingGameModeStartingCardAmount = 1;
    public const int k_AscendingGameModeCardLimit = 6;
    public const int k_DescendingGameModeStartingCardAmount = 5;
    public const int k_DescendingGameModeCardLimit = 4;

    [SerializeField] private AllOpponentCards allOpponentCards;
    [SerializeField] private List<GameObject> cardPrefabs;
    [SerializeField] private GameObject deckGameObject;
    private Dictionary<string, GameObject> m_cardToPrefabMap;
    private List<GameObject> m_cardGameObjects;

    private const float k_xSpacing = 0.06f;
    private const float k_yCenter = 1.1f;
    private const float k_ySpacing = 0.008f;
    private const float k_zCenter = 0.7f;
    private const float k_zSpacing = 0.001f;
    private const int k_maxCardsPerRow = 10;
    private static Quaternion k_cardsFacePlayerRotation = new Quaternion(90, 0, 0, 90);

    private int m_startingCardAmount;
    private int m_endingCardAmount;

    private Deck m_deck;
    private Dictionary<ulong, PlayerCardInfo> m_clientCardInfo;

    private HandsInPlay m_handsInPlay;

    [HideInInspector]
    public delegate void PlayerOutDelegateHandler(ulong losingClientId);
    [HideInInspector]
    public event PlayerOutDelegateHandler OnPlayerOut;

    private void Awake()
    {
        Instance = this;

        m_deck = new Deck();

        m_cardToPrefabMap = new Dictionary<string, GameObject>();

        m_cardGameObjects = new List<GameObject>();

        m_handsInPlay = new HandsInPlay();

        m_clientCardInfo = new Dictionary<ulong, PlayerCardInfo>();

        foreach (GameObject prefab in cardPrefabs)
        {
            string prefabName = prefab.name.Split("_")[2];
            m_cardToPrefabMap[prefabName] = prefab;
        }
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsServer)
        {
            // PokerHandsBullshitGame.Instance.m_allPlayersConnected.OnValueChanged += (oldValue, newValue) =>
            // {
            //     Debug.Log($"Server side detected change in all players connection state, are all players connected? {newValue}");
            //     if (newValue)
            //     {
            //         DistributeCards();
            //     }
            // };

            // InitializePlayerEmptyHand(NetworkManager.Singleton.LocalClientId);
            SceneTransitionHandler.Instance.OnClientLoadedScene += InitializePlayerEmptyHand;
            SceneTransitionHandler.Instance.OnAllClientsLoadedScene += DistributeCards;
            // NetworkManager.OnClientConnectedCallback += InitializePlayerEmptyHand;
            NetworkManager.OnClientDisconnectCallback += RemovePlayer;

            SetAmountOfCardsFromGameSetting();
        }

        PokerHandsBullshitGame.Instance.RegisterCardManagerCallbacks();
        TurnManager.Instance.RegisterCardManagerCallbacks();

    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        if (IsServer)
        {
            SceneTransitionHandler.Instance.OnClientLoadedScene -= InitializePlayerEmptyHand;
            SceneTransitionHandler.Instance.OnAllClientsLoadedScene -= DistributeCards;
            NetworkManager.OnClientDisconnectCallback -= RemovePlayer;

            PokerHandsBullshitGame.Instance.UnregisterCardManagerCallbacks();
            TurnManager.Instance.UnregisterCardManagerCallbacks();
        }
    }

    public void SetAmountOfCardsFromGameSetting()
    {
        if (IsServer)
        {
            if (PokerHandsBullshitGame.Instance.SelectedGameType == PokerHandsBullshitGame.GameType.Ascending)
            {
                m_startingCardAmount = k_AscendingGameModeStartingCardAmount;
                m_endingCardAmount = k_AscendingGameModeCardLimit;
            }
            else
            {
                m_startingCardAmount = k_DescendingGameModeStartingCardAmount;
                m_endingCardAmount = k_DescendingGameModeCardLimit;
            }
        }
    }

    public void CreateCardGameObjects(List<Card> playerCards)
    {
        int numberOfCardsToDisplay = playerCards.Count;
        int rowsOfCards = numberOfCardsToDisplay / k_maxCardsPerRow;
        int cardsPerRow = Math.Min(numberOfCardsToDisplay, k_maxCardsPerRow);
        float xPos = -cardsPerRow * k_xSpacing / 2;
        float yPos = k_yCenter - (rowsOfCards - 1) * k_ySpacing / 2;
        float zPos = k_zCenter - (rowsOfCards - 1) * k_zSpacing / 2;
        for (int i = 0; i < numberOfCardsToDisplay; i++)
        {
            Card currentCard = playerCards[i];
            // TODO: this if statement should not be needed as all cards will have a prefab mapped to it
            if (m_cardToPrefabMap.TryGetValue(currentCard.GetCardIdentifier(), out GameObject cardPrefab))
            {
                int spacingInterval = i / cardsPerRow;
                int cardsNowInRow = i % cardsPerRow;
                if (cardsNowInRow == 0)
                {
                    xPos = -Math.Min(numberOfCardsToDisplay - i, k_maxCardsPerRow) * k_xSpacing / 2;
                    yPos += k_ySpacing * spacingInterval;
                    zPos -= k_zSpacing * spacingInterval;
                }
                GameObject cardGameObject = Instantiate(cardPrefab, new Vector3(xPos, yPos, zPos), k_cardsFacePlayerRotation);
                xPos += k_xSpacing;
                m_cardGameObjects.Add(cardGameObject);
            }
            else
            {
                Debug.LogError($"No prefab found for card identifier {currentCard.GetCardIdentifier()} for {currentCard.GetCardName()}");
            }
        }
    }

    private void CreateOtherPlayersCardsGameObjects(Dictionary<ulong, PlayerCardInfo> otherClientsCardInfo, ulong[] clientOrder)
    {
        // TODO: why not do this server-side? Giving client work to do
        List<int> opponentCardAmounts = new List<int>();
        List<string> opponentNames = new List<string>();
        foreach (ulong clientId in clientOrder)
        {
            PlayerCardInfo otherClientCardInfo = otherClientsCardInfo[clientId];
            opponentCardAmounts.Add(otherClientCardInfo.amountOfCards);
            opponentNames.Add(otherClientCardInfo.playerName);
        }
        allOpponentCards.DisplayOpponentCards(opponentCardAmounts, opponentNames, clientOrder);
    }

    //public void UpdateCardVisual(Card card)
    //{
    //    if (m_cardGameObjects.TryGetValue(card, out GameObject cardGameObject))
    //    {
    //        cardGameObject.transform.Translate(Vector3.forward * 2 + cardGameObject.transform.position);
    //    }
    //    else
    //    {
    //        Debug.Log($"{card.GetCardName()} identifying GameObject can not be found");
    //    }
    //}

    public void InitializePlayerEmptyHand(ulong clientId)
    {
        if (IsServer)
        {
            Debug.Log($"Giving client #{clientId} empty hand");
            if (!m_clientCardInfo.ContainsKey(clientId))
            {
                PlayerCardInfo clientCardInfo = new PlayerCardInfo(new List<Card>(), m_startingCardAmount, PokerHandsBullshitGame.Instance.GetClientName(clientId));
                m_clientCardInfo.Add(clientId, clientCardInfo);
            }
        }
    }

    public void RemovePlayer(ulong clientId)
    {
        if (IsServer)
        {
            if (m_clientCardInfo.ContainsKey(clientId))
            {
                m_clientCardInfo.Remove(clientId);
            }
        }
    }

    public void DistributeCards()
    {
        if (IsServer)
        {
            m_deck.ResetDeck();
            m_deck.Shuffle();
            ulong[] allClientIds = m_clientCardInfo.Keys.ToArray();
            //int[] allClientCardAmounts = m_clientCardInfo.Values.Select(i => i.amountOfCards).ToArray();
            //string[] allClientNames = m_clientCardInfo.Values.Select(i => i.playerName).ToArray();
            PlayerCardInfo[] otherClientCards = m_clientCardInfo.Values.ToArray();
            HashSet<ulong> clientsThatViewDeck = new HashSet<ulong>();
            foreach (KeyValuePair<ulong, PlayerCardInfo> clientIdAndCards in m_clientCardInfo)
            {
                ulong clientId = clientIdAndCards.Key;
                if (!PokerHandsBullshitGame.Instance.IsNotOut(clientId))
                {
                    clientsThatViewDeck.Add(clientId);
                    continue;
                }
                PlayerCardInfo clientCardInfo = clientIdAndCards.Value;
                List<Card> clientsCards = clientCardInfo.cards;
                for (int i = 0; i < clientCardInfo.amountOfCards; i++)
                {
                    Card card = m_deck.TakeCard();
                    clientsCards.Add(card);
                }
            }
            foreach (KeyValuePair<ulong, PlayerCardInfo> clientIdAndCards in m_clientCardInfo)
            {
                ulong clientId = clientIdAndCards.Key;
                PlayerCardInfo clientCardInfo = clientIdAndCards.Value;
                List<Card> clientsCards = clientCardInfo.cards;
                ClientRpcParams clientRpcParams = new ClientRpcParams
                {
                    Send = new ClientRpcSendParams
                    {
                        TargetClientIds = new ulong[] { clientId }
                    }
                };
                List<Card> sendingCards = clientsCards;
                if (clientsThatViewDeck.Contains(clientId))
                {
                    sendingCards = m_deck.m_deck;
                }
                SendCardInfoToPlayerClientRpc(
                    sendingCards.ToArray(),
                    allClientIds,
                    otherClientCards,
                    TurnManager.Instance.GetTurnOrderStartingAtClient(clientId).ToArray(),
                    clientRpcParams
                );
            }
            m_handsInPlay.PopulateCardsInPlay(m_clientCardInfo.Values.SelectMany(i => i.cards));
            m_handsInPlay.FindHandsInPlay();
        }
    }

    public bool IsHandInPlay(PokerHand pokerHand)
    {
        if (IsServer)
        {
            return m_handsInPlay.IsHandInPlay(pokerHand);
        }
        else return false;
    }

    public List<PokerHand> GetAllHandsInPlay()
    {
        if (IsServer)
        {
            return m_handsInPlay.GetHandsInPlay();
        }
        return new List<PokerHand>();
    }

    private void ChangeClientCardAmount(ulong clientId, int cardAmonutChange)
    {
        if (IsServer)
        {
            PlayerCardInfo clientCardInfo = m_clientCardInfo[clientId];
            clientCardInfo.amountOfCards += cardAmonutChange;
            if (clientCardInfo.amountOfCards == m_endingCardAmount)
            {
                Debug.Log($"client {clientId} ({m_endingCardAmount} cards in hand) is out");
                m_clientCardInfo[clientId].cards.Clear();
                clientCardInfo.amountOfCards = 0;
                OnPlayerOut?.Invoke(clientId);
            }
            m_clientCardInfo[clientId] = clientCardInfo;
        }
    }

    //public void EndOfRound(ulong losingClientId, int cardAmonutChange)
    //{
    //    if (IsServer)
    //    {
    //        ChangeClientCardAmount(losingClientId, cardAmonutChange);
    //    }
    //}

    public void NextRound(ulong losingClientId, int cardAmonutChange)
    {
        if (IsServer)
        {
            ChangeClientCardAmount(losingClientId, cardAmonutChange);
            DestroyCardGameObjectsClientRpc();
            foreach (PlayerCardInfo clientCardInfo in m_clientCardInfo.Values)
            {
                clientCardInfo.cards.Clear();
            }
            m_handsInPlay.ResetHandsInPlay();
            DistributeCards();
        }
    }

    public void NewGamePlayerCards(List<ulong> inPlayClientIds)
    {
        if (IsServer)
        {
            SetAmountOfCardsFromGameSetting();
            m_clientCardInfo.Clear();
            foreach (ulong clientId in inPlayClientIds)
            {
                InitializePlayerEmptyHand(clientId);
            }
            DistributeCards();
        }
    }

    [ClientRpc]
    public void SendCardInfoToPlayerClientRpc(
        Card[] clientsCards,
        ulong[] allClientIds,
        PlayerCardInfo[] otherClientsCards,
        ulong[] clientOrder,
        ClientRpcParams clientRpcParams = default
    )
    {
        Debug.Log($"Received list of cards {clientsCards.Length}");
        deckGameObject.SetActive(PokerHandsBullshitGame.Instance.IsNotOut());
        CreateCardGameObjects(clientsCards.ToList());

        Dictionary<ulong, PlayerCardInfo> otherClientsCardInfo = new Dictionary<ulong, PlayerCardInfo>();
        for (int i = 0; i < otherClientsCards.Length; i++)
        {
            if (NetworkManager.Singleton.LocalClientId != allClientIds[i])
            {
                PlayerCardInfo opponentCardInfo = otherClientsCards[i]; // new PlayerCardInfo(null, allClientCardAmounts[i], allClientNames[i]);
                otherClientsCardInfo.Add(allClientIds[i], opponentCardInfo);
            }
        }
        CreateOtherPlayersCardsGameObjects(otherClientsCardInfo, clientOrder.SubArray(1, clientOrder.Length - 1));
    }

    [ClientRpc]
    public void DestroyCardGameObjectsClientRpc()
    {
        Debug.Log($"Destroying the card game objects");
        foreach (GameObject cardGameObject in m_cardGameObjects) Destroy(cardGameObject);
        m_cardGameObjects.Clear();
        allOpponentCards.HideOpponentHands();
    }
}
