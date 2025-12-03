using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;

public class CardGameServerManager
{
    private readonly DeckManager deckManager;
    private readonly Dictionary<ulong, PlayerCardInfo> clientCards = new();
    private HandsInPlay handsInPlay = new();

    private int startAmount, endAmount, loseChange;

    public CardGameServerManager(DeckManager deck)
    {
        deckManager = deck;
    }

    public void RegisterServerEvents()
    {
        SceneTransitionHandler.Instance.OnClientLoadedScene += OnClientLoadedScene;
        TurnManager.Instance.OnTurnOrderDecided += OnTurnOrderDecided;
        NetworkManager.Singleton.OnClientDisconnectCallback += RemovePlayer;
    }

    public void UnregisterServerEvents()
    {
        SceneTransitionHandler.Instance.OnClientLoadedScene -= OnClientLoadedScene;
        TurnManager.Instance.OnTurnOrderDecided -= OnTurnOrderDecided;
        NetworkManager.Singleton.OnClientDisconnectCallback -= RemovePlayer;
    }

    private void OnClientLoadedScene(ulong clientId)
    {
        InitializePlayerEmptyHand(clientId);
        // TryDistribute();
    }

    private void OnTurnOrderDecided() => TryDistribute();

    private void TryDistribute()
    {
        if (StartingConditionsForCardDistribution())
        {
            DistributeCards();
        }
    }

    public bool StartingConditionsForCardDistribution()
    {
        return clientCards.Count == GameManager.Instance.NumberOfPlayers;
    }

    public void InitializePlayerEmptyHand(ulong clientId)
    {
        if (clientCards.ContainsKey(clientId)) return;
        string playerName = GameManager.Instance.GetClientName(clientId);
        clientCards[clientId] = new PlayerCardInfo(new List<Card>(), startAmount, playerName);
    }

    public void ClearAllHands()
    {
        foreach (ulong clientId in clientCards.Keys)
        {
            ClearPlayerHand(clientId);
        }
    }

    public void ClearPlayerHand(ulong clientId)
    {
        if (clientCards.TryGetValue(clientId, out PlayerCardInfo clientCardInfo))
        {
            clientCardInfo.cards.Clear();
        }
    }

    public void ClearPlayers()
    {
        clientCards.Clear();
    }

    public bool IsHandInPlay(PokerHand pokerHand)
    {
        return handsInPlay.IsHandInPlay(pokerHand);
    }

    public List<PokerHand> GetHandsInPlay()
    {
        return handsInPlay.GetHandsInPlay();
    }

    public bool ChangeClientCardAmount(ulong clientId)
    {
        bool isPlayerOut = false;
        PlayerCardInfo clientCardInfo = clientCards[clientId];
        clientCardInfo.amountOfCards += loseChange;
        if (clientCardInfo.amountOfCards == endAmount)
        {
            clientCards[clientId].cards.Clear();
            clientCardInfo.amountOfCards = 0;
            isPlayerOut = true;
        }
        clientCards[clientId] = clientCardInfo;
        return isPlayerOut;
    }

    public void RemovePlayer(ulong clientId)
    {
        clientCards.Remove(clientId);
    }

    public void ResetHandsInPlay()
    {
        handsInPlay.ResetHandsInPlay();
    }

    public void ConfigureFromGameSettings()
    {
        if (GameManager.Instance.SelectedGameType == GameType.Ascending)
        {
            startAmount = CardManagerConstants.AscStarting;
            endAmount = CardManagerConstants.AscLimit;
            loseChange = CardManagerConstants.AscLoseChange;
        }
        else
        {
            startAmount = CardManagerConstants.DescStarting;
            endAmount = CardManagerConstants.DescLimit;
            loseChange = CardManagerConstants.DescLoseChange;
        }
    }

    public void DistributeCards()
    {
        deckManager.ResetAndShuffle();

        HashSet<ulong> clientsThatViewDeck = new();
        foreach (var kvp in clientCards)
        {
            var clientId = kvp.Key;
            var info = kvp.Value;
            info.cards.Clear();
            if (!GameManager.Instance.m_inPlayClientIds.Contains(clientId))
            {
                clientsThatViewDeck.Add(clientId);
                continue;
            }
            else
            {
                for (int i = 0; i < info.amountOfCards; i++)
                    info.cards.Add(deckManager.DrawCard());
            }
        }

        foreach (var kvp in clientCards)
        {
            var clientId = kvp.Key;

            List<Card> sendingCards = kvp.Value.cards;
            if (clientsThatViewDeck.Contains(clientId))
            {
                sendingCards = deckManager.GetAllCardsInDeck();
            }

            CardManager.Instance.SendCardInfoToPlayerClientRpc(
                sendingCards.ToArray(),
                clientCards.Keys.ToArray(),
                clientCards.Values.ToArray(),
                TurnManager.Instance.GetTurnOrderStartingAtClient(clientId).ToArray(),
                GetTotalCardsInPlay() <= CardManagerConstants.FlushLimit,
                new ClientRpcParams { Send = new() { TargetClientIds = new[] { clientId } } }
            );
        }

        handsInPlay.PopulateCardsInPlay(clientCards.Values.SelectMany(v => v.cards));
        handsInPlay.FindHandsInPlay();
    }

    public int GetTotalCardsInPlay() => clientCards.Values.Sum(i => i.amountOfCards);
}
