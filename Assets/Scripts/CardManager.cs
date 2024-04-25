using CardTraitExtensions;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class CardManager : NetworkBehaviour
{
    public static CardManager Instance {  get; private set; }
    
    public const int k_AscendingGameModeStartingCardAmount = 1;
    public const int k_AscendingGameModeCardLimit = 6;
    public const int k_DescendingGameModeStartingCardAmount = 5;
    public const int k_DescendingGameModeCardLimit = 0;

    [SerializeField] private List<GameObject> cardPrefabs;
    private Dictionary<string, GameObject> m_cardToPrefabMap;
    private List<GameObject> m_cardGameObjects;

    private float m_cardSpaceWidth = 0.06f;
    private int m_startingCardAmount;
    private int m_endingCardAmount;

    private Deck m_deck;
    // private Dictionary<ulong, Hand> m_playerHands = new Dictionary<ulong, Hand>();
    // private Dictionary<ulong, PlayerController> m_playerControllers = new Dictionary<ulong, PlayerController>();
    private Dictionary<ulong, int> m_playerNumberOfCardsInHand = new Dictionary<ulong, int>();
    private Dictionary<ulong, List<Card>> m_playerCards = new Dictionary<ulong, List<Card>>();

    private HandsInPlay m_handsInPlay;

    private void Awake()
    {
        Instance = this;

        m_deck = new Deck();

        m_cardToPrefabMap = new Dictionary<string, GameObject>();

        m_cardGameObjects = new List<GameObject>();

        m_handsInPlay = new HandsInPlay();

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
            // TODO
            // PokerHandsBullshitGame.Instance.GetTurnOrder();
            // SendCardAmountToPlayersClientRpc(m_playerNumberOfCardsInHand.Keys.ToArray(), m_playerNumberOfCardsInHand.Values.ToArray());
            m_handsInPlay.PopulateCardsInPlay(m_playerCards.Values.SelectMany(i => i));
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
            m_handsInPlay.ResetHandsInPlay();
            DistributeCards();
        }
    }

    [ClientRpc]
    public void SendCardsToPlayerClientRpc(Card[] playersCards, ClientRpcParams clientRpcParams = default)
    {
        Debug.Log($"Received list of cards {playersCards.Length}");
        CreateCardGameObjects(playersCards.ToList());
    }

    // TODO: for displaying other players' card amounts in clients' game
    //[ClientRpc]
    //public void SendCardAmountToPlayersClientRpc(ulong[] ids, int[] amountOfCards)
    //{
    //    if (NetworkManager.LocalClientId != playerId)
    //    {
    //        CreateOtherPlayersCardsGameObjects(amountOfCards, playerName, relativeTurnOrder);
    //    }
    //}

    [ClientRpc]
    public void DestroyCardGameObjectsClientRpc()
    {
        Debug.Log($"Destroying the card game objects");
        foreach (GameObject cardGameObject in m_cardGameObjects) Destroy(cardGameObject);
        m_cardGameObjects.Clear();
    }
}
