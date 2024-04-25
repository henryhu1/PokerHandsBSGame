using CardTraitExtensions;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class CardManager : NetworkBehaviour
{
    public static CardManager Instance {  get; private set; }

    [SerializeField] private List<GameObject> cardPrefabs;
    private Dictionary<string, GameObject> m_cardToPrefabMap;
    private List<GameObject> m_cardGameObjects;

    private float m_cardSpaceWidth = 0.06f;
    private int m_startingCardAmount;
    public int StartingCardAmount
    {
        get { return m_startingCardAmount; }
        set { m_startingCardAmount = value; }
    }

    private Deck m_deck;
    // private Dictionary<ulong, Hand> m_playerHands = new Dictionary<ulong, Hand>();
    // private Dictionary<ulong, PlayerController> m_playerControllers = new Dictionary<ulong, PlayerController>();
    private Dictionary<ulong, int> m_playerNumberOfCardsInHand = new Dictionary<ulong, int>();
    private Dictionary<ulong, List<Card>> m_playerCards = new Dictionary<ulong, List<Card>>();

    public class CardsInPlay
    {
        private int m_numberOfCardsInPlay;
        private Dictionary<Rank, uint> m_rankCount;
        private Dictionary<Suit, Dictionary<Rank, uint>> m_bySuitRankCount;
        private SortedSet<PokerHand> m_handsInPlay;

        public CardsInPlay()
        {
            ResetCardsInPlay();
        }

        private Dictionary<Rank, uint> GetEmptyDictionary()
        {
            return new Dictionary<Rank, uint> {
                { Rank.Two, 0 },
                { Rank.Three, 0 },
                { Rank.Four, 0 },
                { Rank.Five, 0 },
                { Rank.Six, 0 },
                { Rank.Seven, 0 },
                { Rank.Eight, 0 },
                { Rank.Nine, 0 },
                { Rank.Ten, 0 },
                { Rank.Jack, 0 },
                { Rank.Queen, 0 },
                { Rank.King, 0 },
                { Rank.Ace, 0 },
            };
        }

        public void ResetCardsInPlay()
        {
            m_numberOfCardsInPlay = 0;
            m_rankCount = GetEmptyDictionary();
            m_bySuitRankCount = new Dictionary<Suit, Dictionary<Rank, uint>>
            {
                { Suit.Spade, GetEmptyDictionary() },
                { Suit.Heart, GetEmptyDictionary() },
                { Suit.Diamond, GetEmptyDictionary() },
                { Suit.Club, GetEmptyDictionary() },
            };
            m_handsInPlay = new SortedSet<PokerHand>();
        }

        public void PopulateCardsInPlay(IEnumerable<Card> allCards)
        {
            foreach (Card card in allCards)
            {
                AddInPlayCard(card);
            }
        }

        public void AddInPlayCard(Card card)
        {
            m_numberOfCardsInPlay++;
            m_rankCount[card.Rank]++;
            m_bySuitRankCount[card.Suit][card.Rank]++;
        }

        private bool CheckRanks(List<Rank> ranks, Dictionary<Rank, uint> rankCounts)
        {
            foreach (Rank rank in ranks)
            {
                if (rankCounts[rank] == 0)
                {
                    return false;
                }
            }
            return true;
        }

        private bool AreRanksInPlay(List<Rank> ranks)
        {
            return CheckRanks(ranks, m_rankCount);
        }
        
        private bool AreSuitedRanksInPlay(List<Rank> ranks, Suit suit)
        {
            return CheckRanks(ranks, m_bySuitRankCount[suit]);
        }

        public void FindExistingHands()
        {
            SortedSet<Pair> pairsInPlay = new SortedSet<Pair>();
            SortedSet<ThreeOfAKind> triplesInPlay = new SortedSet<ThreeOfAKind>();
            foreach (KeyValuePair<Rank, uint> rankCount in m_rankCount)
            {
                if (rankCount.Value >= 1)
                {
                    m_handsInPlay.Add(new HighCard(rankCount.Key));
                }
                if (rankCount.Value >= 2)
                {
                    Pair pairInPlay = new Pair(rankCount.Key);
                    m_handsInPlay.Add(pairInPlay);
                    pairsInPlay.Add(pairInPlay);
                }
                if (rankCount.Value >= 3)
                {
                    ThreeOfAKind tripleInPlay = new ThreeOfAKind(rankCount.Key);
                    m_handsInPlay.Add(tripleInPlay);
                    triplesInPlay.Add(tripleInPlay);
                }
                if (rankCount.Value >= 4)
                {
                    m_handsInPlay.Add(new FourOfAKind(rankCount.Key));
                }
            }

            for (int i = 0; i < pairsInPlay.Count; i++)
            {
                for (int j = i + 1; j < pairsInPlay.Count; j++)
                {
                    m_handsInPlay.Add(new TwoPair(pairsInPlay.ElementAt(j), pairsInPlay.ElementAt(i)));
                }
                for (int  k = 0; k < triplesInPlay.Count; k++)
                {
                    m_handsInPlay.Add(new FullHouse(triplesInPlay.ElementAt(k), pairsInPlay.ElementAt(i)));
                }
            }

            for (Rank highestInStraight = Straight.s_LowestStraight; highestInStraight <= Rank.Ace; highestInStraight += 1)
            {
                List<Rank> straight = highestInStraight.GetStraight();
                if (AreRanksInPlay(straight))
                {
                    m_handsInPlay.Add(new Straight(highestInStraight));
                }
            }

            foreach (KeyValuePair<Suit, Dictionary<Rank, uint>> suitRanks in m_bySuitRankCount)
            {
                Suit checkingSuit = suitRanks.Key;

                if (m_numberOfCardsInPlay <= 15)
                {
                    Dictionary<Rank, uint> ranksInPlay = suitRanks.Value;
                    uint count = 0;
                    foreach (KeyValuePair<Rank, uint> rankInPlay in ranksInPlay)
                    {
                        if (rankInPlay.Value > 0)
                        {
                            count += rankInPlay.Value;
                            if (count >= 5)
                            {
                                m_handsInPlay.Add(new Flush(rankInPlay.Key, checkingSuit));
                            }
                        }
                    }
                }

                for (Rank highestInStraight = Straight.s_LowestStraight; highestInStraight <= Rank.King; highestInStraight += 1)
                {
                    List<Rank> straight = highestInStraight.GetStraight();
                    if (AreSuitedRanksInPlay(straight, checkingSuit))
                    {
                        m_handsInPlay.Add(new StraightFlush(highestInStraight, checkingSuit));
                    }
                }

                List<Rank> royalFlush = Rank.Ace.GetStraight();
                if (AreSuitedRanksInPlay(royalFlush, checkingSuit))
                {
                    m_handsInPlay.Add(new RoyalFlush(checkingSuit));
                }
            }

            //StringBuilder stringBuilder = new StringBuilder();
            //stringBuilder.AppendLine("Hands in play:");
            //for (int i = 0; i < m_handsInPlay.Count; i++)
            //{
            //    stringBuilder.Append($"{m_handsInPlay.ElementAt(i).GetStringRepresentation()}");
            //    if (i < m_handsInPlay.Count - 1)
            //    {
            //        stringBuilder.Append(", ");
            //    }
            //}
            //Debug.Log(stringBuilder.ToString());
        }

        public bool IsHandInPlay(PokerHand pokerHand)
        {
            return m_handsInPlay.Contains(pokerHand);
        }
    }

    private CardsInPlay m_cardsInPlay;

    private void Awake()
    {
        Instance = this;

        m_deck = new Deck();

        m_cardToPrefabMap = new Dictionary<string, GameObject>();

        m_cardGameObjects = new List<GameObject>();

        m_cardsInPlay = new CardsInPlay();

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
            // NetworkManager.OnClientConnectedCallback += InitializePlayerEmptyHand;
            NetworkManager.OnClientDisconnectCallback += RemovePlayer;
        }
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        if (IsServer)
        {
            SceneTransitionHandler.Instance.OnClientLoadedScene -= InitializePlayerEmptyHand;
            // NetworkManager.OnClientConnectedCallback -= InitializePlayerEmptyHand;
            NetworkManager.OnClientDisconnectCallback -= RemovePlayer;
        }
    }

    public void CreateCardGameObjects(List<Card> playerCards)
    {
        int playerCardCount = playerCards.Count;
        float cardSpace = m_cardSpaceWidth * playerCardCount;
        for (int i = 0; i < playerCardCount; i++)
        {
            Card currentCard = playerCards[i];
            if (m_cardToPrefabMap.TryGetValue(currentCard.GetCardIdentifier(), out GameObject cardPrefab))
            {
                GameObject cardGameObject = Instantiate(cardPrefab, new Vector3(-cardSpace / 2 + i * m_cardSpaceWidth, 1.1f, 0.7f), new Quaternion(90, 0, 0, 90));
                m_cardGameObjects.Add(cardGameObject);
            }
            else
            {
                Debug.LogError($"No prefab found for card identifier {currentCard.GetCardIdentifier()} for {currentCard.GetCardName()}");
            }
        }
    }

    public void CreateOtherPlayersCardsGameObjects(int amountOfCards, string playerName, int turnIndex)
    {

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
            Debug.Log($"Giving client #{clientId} cards");
            if (!m_playerNumberOfCardsInHand.ContainsKey(clientId))
            {
                m_playerNumberOfCardsInHand.Add(clientId, m_startingCardAmount);
            }
            if (!m_playerCards.ContainsKey(clientId))
            {
                m_playerCards.Add(clientId, new List<Card>());
            }
            if (SceneTransitionHandler.Instance.AllClientsAreLoaded())
            {
                DistributeCards();
            }
        }
    }

    public void RemovePlayer(ulong clientId)
    {
        if (IsServer)
        {
            if (m_playerCards.ContainsKey(clientId))
            {
                m_playerCards.Remove(clientId);
            }
        }
    }

    public void DistributeCards()
    {
        if (IsServer)
        {
            m_deck.ResetDeck();
            m_deck.Shuffle();
            foreach (KeyValuePair<ulong, List<Card>> playerIdAndCards in m_playerCards)
            {
                ulong playerId = playerIdAndCards.Key;
                List<Card> playerCards = playerIdAndCards.Value;
                for (int i = 0; i < m_playerNumberOfCardsInHand[playerId]; i++)
                {
                    Card card = m_deck.TakeCard();
                    playerCards.Add(card);
                }
                ClientRpcParams clientRpcParams = new ClientRpcParams
                {
                    Send = new ClientRpcSendParams
                    {
                        TargetClientIds = new ulong[] { playerId }
                    }
                };
                SendCardsToPlayerClientRpc(playerCards.ToArray(), clientRpcParams);
            }
            PokerHandsBullshitGame.Instance.GetTurnOrder();
            SendCardAmountToPlayersClientRpc(m_playerNumberOfCardsInHand.Keys.ToArray(), m_playerNumberOfCardsInHand.Values.ToArray());
            m_cardsInPlay.PopulateCardsInPlay(m_playerCards.Values.SelectMany(i => i));
            m_cardsInPlay.FindExistingHands();
        }
    }

    public bool IsHandInPlay(PokerHand pokerHand)
    {
        if (IsServer)
        {
            return m_cardsInPlay.IsHandInPlay(pokerHand);
        }
        else return false;
    }

    private void ChangePlayerCardAmount(ulong playerID, int cardAmonutChange)
    {
        if (IsServer)
        {
            m_playerNumberOfCardsInHand[playerID] += cardAmonutChange;
        }
    }

    public void EndOfRound(ulong losingPlayerID, int cardAmonutChange)
    {
        if (IsServer)
        {
            DestroyCardGameObjectsClientRpc();
            ChangePlayerCardAmount(losingPlayerID, cardAmonutChange);
            foreach (List<Card> playersCards in m_playerCards.Values)
            {
                playersCards.Clear();
            }
            m_cardsInPlay.ResetCardsInPlay();
            DistributeCards();
        }
    }

    [ClientRpc]
    public void SendCardsToPlayerClientRpc(Card[] playersCards, ClientRpcParams clientRpcParams = default)
    {
        Debug.Log($"Received list of cards {playersCards.Length}");
        CreateCardGameObjects(playersCards.ToList());
    }

    [ClientRpc]
    public void SendCardAmountToPlayersClientRpc(ulong[] ids, int[] amountOfCards)
    {
        if (NetworkManager.LocalClientId != playerId)
        {
            CreateOtherPlayersCardsGameObjects(amountOfCards, playerName, relativeTurnOrder);
        }
    }

    [ClientRpc]
    public void DestroyCardGameObjectsClientRpc()
    {
        Debug.Log($"Destroying the card game objects");
        foreach (GameObject cardGameObject in m_cardGameObjects) Destroy(cardGameObject);
        m_cardGameObjects.Clear();
    }
}
