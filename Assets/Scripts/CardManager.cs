using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

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

    [SerializeField] GraphicRaycaster graphicRaycaster;

    public const int k_AscendingGameModeStartingCardAmount = 1;
    public const int k_AscendingGameModeCardLimit = 6;
    public const int k_AscendingGameModeLosingCardChange = 1;
    public const int k_DescendingGameModeStartingCardAmount = 5;
    public const int k_DescendingGameModeCardLimit = 0;
    public const int k_DescendingGameModeLosingCardChange = -1;
    public const int k_FlushAllowedAmountOfCards = 15;

    [SerializeField] private AllOpponentCards allOpponentCards;
    [SerializeField] private List<GameObject> cardPrefabs; // can this be changed to a list of draggables?
    [SerializeField] private GameObject deckGameObject;
    private Dictionary<string, GameObject> m_cardToPrefabMap;
    private List<Card> m_myCards;
    private List<Draggable> m_cardGameObjects;
    private List<Vector3> m_cardGameObjectPositions;
    private int m_cardDraggingIndex = -1;
    private int m_cardDraggingOriginalIndex = -1;

    private const float k_xSpacing = 0.06f;
    private const float k_yCenter = 1.1f;
    private const float k_ySpacing = 0.02f;
    private const float k_zCenter = 0.75f;
    private const float k_zSpacing = 0.001f;
    private const int k_maxCardsPerRow = 10;
    private const float k_cardMovementDuration = 0.2f;
    private static Vector3 k_cardZoom = Vector3.forward * 0.01f;
    private static Quaternion k_cardsFacePlayerRotation = new Quaternion(90, 0, 0, 90);

    private int m_startingCardAmount;
    private int m_endingCardAmount;
    private int m_loserCardAmountChange;
    public bool m_areCardsSorted { get; private set; }

    private Deck m_deck;
    private Dictionary<ulong, PlayerCardInfo> m_clientCardInfo;

    private HandsInPlay m_handsInPlay;

    private bool m_startingTurnOrderDecided;

    [HideInInspector]
    public delegate void AreFlushesAllowedHandsDelegateHandler(bool flushesAllowed);
    [HideInInspector]
    public event AreFlushesAllowedHandsDelegateHandler OnAreFlushesAllowed;

    [HideInInspector]
    public delegate void PlayerOutDelegateHandler(ulong losingClientId);
    [HideInInspector]
    public event PlayerOutDelegateHandler OnPlayerOut;

    private void Awake()
    {
        if (Instance != this && Instance != null)
        {
            Destroy(Instance.gameObject);
        }
        Instance = this;

        m_startingTurnOrderDecided = false;
        m_deck = new Deck();
        m_cardToPrefabMap = new Dictionary<string, GameObject>();
        m_cardGameObjects = new List<Draggable>();
        m_cardGameObjectPositions = new();
        m_handsInPlay = new HandsInPlay();
        m_clientCardInfo = new Dictionary<ulong, PlayerCardInfo>();

        foreach (GameObject prefab in cardPrefabs)
        {
            string prefabName = prefab.name.Split("_")[2];
            m_cardToPrefabMap[prefabName] = prefab;
        }
    }

    private void Start()
    {
        OrderCardsUI.Instance.OnOrderCards += OrderCardsUI_OrderCards;
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        OrderCardsUI.Instance.OnOrderCards -= OrderCardsUI_OrderCards;
    }

    private void OrderCardsUI_OrderCards(bool isDoAscendingSort)
    {
        DestroyCardGameObjects();
        m_myCards.Sort();
        if (!isDoAscendingSort)
        {
            m_myCards.Reverse();
        }
        CreateCardGameObjects();
        m_areCardsSorted = true;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsServer)
        {
            SceneTransitionHandler.Instance.OnClientLoadedScene += SceneTransitionHandler_ClientLoadedScene;
            NetworkManager.OnClientDisconnectCallback += RemovePlayer;
            TurnManager.Instance.OnTurnOrderDecided += TurnManager_TurnOrderDecided;

            GameManager.Instance.RegisterCardManagerCallbacks();
            TurnManager.Instance.RegisterCardManagerCallbacks();

            SetAmountOfCardsFromGameSetting();
        }
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        if (IsServer)
        {
            SceneTransitionHandler.Instance.OnClientLoadedScene -= SceneTransitionHandler_ClientLoadedScene;
            NetworkManager.OnClientDisconnectCallback -= RemovePlayer;
            TurnManager.Instance.OnTurnOrderDecided -= TurnManager_TurnOrderDecided;

            GameManager.Instance.UnregisterCardManagerCallbacks();
            TurnManager.Instance.UnregisterCardManagerCallbacks();
        }
    }

    private void SceneTransitionHandler_ClientLoadedScene(ulong clientId)
    {
        if (IsServer)
        {
            InitializePlayerEmptyHand(clientId);
            if (StartingConditionsForCardDistribution())
            {
                DistributeCards();
            }
        }
    }

    private void TurnManager_TurnOrderDecided()
    {
        if (IsServer)
        {
            m_startingTurnOrderDecided = true;
            if (StartingConditionsForCardDistribution())
            {
                DistributeCards();
            }
        }
    }

    private bool StartingConditionsForCardDistribution()
    {
        return m_startingTurnOrderDecided && m_clientCardInfo.Count == GameManager.Instance.NumberOfPlayers;
    }

    public void SetAmountOfCardsFromGameSetting()
    {
        if (IsServer)
        {
            if (GameManager.Instance.SelectedGameType == GameType.Ascending)
            {
                m_startingCardAmount = k_AscendingGameModeStartingCardAmount;
                m_endingCardAmount = k_AscendingGameModeCardLimit;
                m_loserCardAmountChange = k_AscendingGameModeLosingCardChange;
            }
            else
            {
                m_startingCardAmount = k_DescendingGameModeStartingCardAmount;
                m_endingCardAmount = k_DescendingGameModeCardLimit;
                m_loserCardAmountChange = k_DescendingGameModeLosingCardChange;
            }
        }
    }

    public void DestroyCardGameObjects()
    {
        foreach (Draggable cardGameObject in m_cardGameObjects) Destroy(cardGameObject.gameObject);
        m_cardGameObjects.Clear();
        m_cardGameObjectPositions.Clear();
        m_areCardsSorted = false;
    }

    public void SetCardEmptySlotPosition(Draggable dragging)
    {
        m_cardDraggingIndex = dragging.Index;
        m_cardDraggingOriginalIndex = dragging.Index;
        graphicRaycaster.enabled = false;
    }

    public void HandleCardDrag(Vector3 pos)
    {
        Vector3 slot;
        for (int i = 0; i < m_cardGameObjectPositions.Count; i++)
        {
            if (i == m_cardDraggingIndex) continue;
            slot = m_cardGameObjectPositions[i];
            if (Math.Abs(slot.x - pos.x) < 0.01 && Math.Abs(slot.y - pos.y) < 0.01)
            {
                Draggable draggingCard = m_cardGameObjects[m_cardDraggingIndex];
                Draggable displacedCard = m_cardGameObjects[i];
                Vector3 emptySpace = m_cardGameObjectPositions[m_cardDraggingIndex];
                StartCoroutine(MoveCard(displacedCard, emptySpace));
                m_cardGameObjects[m_cardDraggingIndex] = displacedCard;
                displacedCard.Index = m_cardDraggingIndex;
                m_cardGameObjects[i] = draggingCard;
                draggingCard.Index = i;
                m_cardDraggingIndex = i;
            }
        }
    }

    public void HandleCardEndDrag(int index)
    {
        if (m_cardDraggingIndex != index) return;
        Draggable exitedCard = m_cardGameObjects[index];
        StartCoroutine(MoveCard(exitedCard, m_cardGameObjectPositions[index], k_cardMovementDuration, true));
        if (m_cardDraggingOriginalIndex != index)
        {
            m_areCardsSorted = false;
        }
        graphicRaycaster.enabled = true;
    }

    public void HandleCardEnter(int index)
    {
        if (m_cardDraggingIndex != -1) return;
        Draggable enteredCard = m_cardGameObjects[index];
        StartCoroutine(MoveCard(enteredCard, m_cardGameObjectPositions[index] + k_cardZoom, k_cardMovementDuration));
    }

    public void HandleCardExit(int index)
    {
        if (m_cardDraggingIndex != -1 || m_cardDraggingIndex == index) return;
        Draggable exitedCard = m_cardGameObjects[index];
        StartCoroutine(MoveCard(exitedCard, m_cardGameObjectPositions[index], k_cardMovementDuration));
    }

    private IEnumerator MoveCard(Draggable card, Vector3 finalPosition, float duration = k_cardMovementDuration, bool isEndDrag = false)
    {
        Vector3 start = card.transform.position;
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;

            float step = time / duration;
            //float curveStep = movementCurve.Evaluate(step);
            card.transform.position = Vector3.Lerp(start, finalPosition, step);

            yield return null;
        }

        card.transform.position = finalPosition;
        if (isEndDrag) m_cardDraggingIndex = -1;
    }

    public void CreateCardGameObjects()
    {
        int numberOfCardsToDisplay = m_myCards.Count;
        int rowsOfCards = numberOfCardsToDisplay / k_maxCardsPerRow;
        int cardsPerRow = Math.Min(numberOfCardsToDisplay, k_maxCardsPerRow);
        float xPos = -cardsPerRow * k_xSpacing / 2;
        float yPos = k_yCenter - (rowsOfCards - 1) * k_ySpacing / 2;
        float zPos = k_zCenter - (rowsOfCards - 1) * k_zSpacing / 2;
        for (int i = 0; i < numberOfCardsToDisplay; i++)
        {
            Card currentCard = m_myCards[i];
            // TODO: this if statement should not be needed as all cards will have a prefab mapped to it
            if (m_cardToPrefabMap.TryGetValue(currentCard.GetCardIdentifier(), out GameObject cardPrefab))
            {
                int cardsNowInRow = i % cardsPerRow;
                if (cardsNowInRow == 0)
                {
                    xPos = -Math.Min(numberOfCardsToDisplay - i, k_maxCardsPerRow) * k_xSpacing / 2;
                    yPos += k_ySpacing;
                    zPos -= k_zSpacing;
                }
                Vector3 cardSlot = new(xPos, yPos, zPos);
                m_cardGameObjectPositions.Add(cardSlot);
                GameObject cardGameObject = Instantiate(cardPrefab, cardSlot, k_cardsFacePlayerRotation);
                Draggable draggable = cardGameObject.GetComponent<Draggable>();
                draggable.Index = m_cardGameObjects.Count;
                xPos += k_xSpacing;
                m_cardGameObjects.Add(draggable);
            }
            else
            {
#if UNITY_EDITOR
                Debug.LogError($"No prefab found for card identifier {currentCard.GetCardIdentifier()} for {currentCard.GetCardName()}");
#endif
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

    // TODO: replace blank cards with opponents' actual cards when round ends
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
            if (!m_clientCardInfo.ContainsKey(clientId))
            {
#if UNITY_EDITOR
                Debug.Log($"Giving client #{clientId} empty hand");
#endif
                PlayerCardInfo clientCardInfo = new PlayerCardInfo(new List<Card>(), m_startingCardAmount, GameManager.Instance.GetClientName(clientId));
                m_clientCardInfo.Add(clientId, clientCardInfo);
            }
            else
            {
#if UNITY_EDITOR
                Debug.LogError($"client #{clientId} already existed and had a hand...");
#endif
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
            int totalCardsInPlay = 0;
            foreach (KeyValuePair<ulong, PlayerCardInfo> clientIdAndCards in m_clientCardInfo)
            {
                ulong clientId = clientIdAndCards.Key;
                if (!GameManager.Instance.m_inPlayClientIds.Contains(clientId))
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
                totalCardsInPlay += clientCardInfo.amountOfCards;
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
#if UNITY_EDITOR
                Debug.Log($"client #{clientId} gets to see {sendingCards.Count} card(s)");
#endif
                SendCardInfoToPlayerClientRpc(
                    sendingCards.ToArray(),
                    allClientIds,
                    otherClientCards,
                    TurnManager.Instance.GetTurnOrderStartingAtClient(clientId).ToArray(),
                    totalCardsInPlay <= k_FlushAllowedAmountOfCards,
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

    public int GetTotalCardsInPlay()
    {
        if (IsServer)
        {
            return m_clientCardInfo.Values.Select(i => i.amountOfCards).Sum();
        }
        return 0;
    }

    public bool IsFlushAllowedToBePlayed()
    {
        if (IsServer)
        {
            return GetTotalCardsInPlay() <= k_FlushAllowedAmountOfCards;
        }
        return false;
    }

    public void ChangeClientCardAmount(ulong clientId)
    {
        if (IsServer)
        {
            PlayerCardInfo clientCardInfo = m_clientCardInfo[clientId];
            clientCardInfo.amountOfCards += m_loserCardAmountChange;
            if (clientCardInfo.amountOfCards == m_endingCardAmount)
            {
#if UNITY_EDITOR
                Debug.Log($"client {clientId} ({m_endingCardAmount} cards in hand) is out");
#endif
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

    public void NextRound()
    {
        if (IsServer)
        {
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
            DestroyCardGameObjectsClientRpc();
            SetAmountOfCardsFromGameSetting();
            m_clientCardInfo.Clear();
            foreach (ulong clientId in inPlayClientIds)
            {
                if (!m_clientCardInfo.ContainsKey(clientId))
                {
                    InitializePlayerEmptyHand(clientId);
                }
            }
            m_handsInPlay.ResetHandsInPlay();
            DistributeCards();
        }
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

#if UNITY_EDITOR
        Debug.Log($"Received list of cards {clientsCards.Length}");
#endif
        deckGameObject.SetActive(GameManager.Instance.m_inPlayClientIds.Contains(NetworkManager.Singleton.LocalClientId));
        m_myCards = clientsCards.ToList();
        CreateCardGameObjects();
        m_areCardsSorted = false;

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
#if UNITY_EDITOR
        Debug.Log($"Destroying the card game objects");
#endif
        m_myCards.Clear();
        DestroyCardGameObjects();
        allOpponentCards.HideOpponentHands();
    }
}
