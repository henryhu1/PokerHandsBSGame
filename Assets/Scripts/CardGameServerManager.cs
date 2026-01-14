using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;

public class CardGameServerManager
{
    private DeckManager deckManager;

    private readonly Dictionary<ulong, PlayerCardInfo> clientCards = new();
    private HandsInPlay handsInPlay = new();

    private int startAmount, endAmount, loseChange;

    public CardGameServerManager()
    {
        deckManager = new DeckManager();
    }

    private void ConfigureForGameType(GameType gameType)
    {
        if (gameType == GameType.Ascending)
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

    private void InitializePlayerEmptyHand(ulong clientId)
    {
        if (clientCards.ContainsKey(clientId)) return;
        string playerName = GameManager.Instance.GetClientName(clientId);
        clientCards[clientId] = new PlayerCardInfo(new List<Card>(), startAmount, playerName, clientId);
    }

    private void ClearAllHands()
    {
        foreach (ulong clientId in clientCards.Keys)
        {
            ClearPlayerHand(clientId);
        }
    }

    private void ClearPlayerHand(ulong clientId)
    {
        if (clientCards.TryGetValue(clientId, out PlayerCardInfo clientCardInfo))
        {
            clientCardInfo.cards.Clear();
        }
    }

    private void ClearPlayers()
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

    public void SetUp(GameType gameType, ulong[] inPlayClientIds)
    {
        ClearPlayers();
        ConfigureForGameType(gameType);

        foreach(ulong clientId in inPlayClientIds)
        {
            InitializePlayerEmptyHand(clientId);
        }
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

    public void SetPlayerOut(ulong clientId)
    {
        PlayerCardInfo clientCardInfo = clientCards[clientId];
        clientCardInfo.amountOfCards = 0;
        clientCardInfo.cards.Clear();
        clientCards[clientId] = clientCardInfo;
    }

    public void RemovePlayer(ulong clientId)
    {
        clientCards.Remove(clientId);
    }

    public PlayerHiddenCardInfo[] GetHiddenOtherPlayersCards()
    {
        return clientCards.Values.Select(
            clientCardInfo => new PlayerHiddenCardInfo(clientCardInfo)
        ).ToArray();
    }

    public void DistributeCards()
    {
        ClearAllHands();
        handsInPlay.ResetHandsInPlay();
        deckManager.ResetAndShuffle();

        foreach (var kvp in clientCards)
        {
            var clientId = kvp.Key;
            var info = kvp.Value;
            info.cards.Clear();

            // How cards are drew doesn't really matter
            for (int i = 0; i < info.amountOfCards; i++)
                info.cards.Add(deckManager.DrawCard());
        }

        PlayerHiddenCardInfo[] hiddenClientCards = GetHiddenOtherPlayersCards();

        foreach (var kvp in clientCards)
        {
            var clientId = kvp.Key;

            List<Card> sendingCards = kvp.Value.cards;

            CardManager.Instance.DistributeCardInfoToPlayerClientRpc(
                sendingCards.ToArray(),
                hiddenClientCards,
                TurnManager.Instance.GetTurnOrderStartingAtClient(clientId).ToArray(),
                GetTotalCardsInPlay() <= CardManagerConstants.FlushLimit,
                new ClientRpcParams { Send = new() { TargetClientIds = new[] { clientId } } }
            );
        }

        handsInPlay.PopulateCardsInPlay(clientCards.Values.SelectMany(v => v.cards));
        handsInPlay.FindHandsInPlay();
    }

    public void DistributeRemainingCardsInDeck(ulong[] spectatingClients)
    {
        List<Card> sendingCards = deckManager.GetAllCardsInDeck();

        PlayerHiddenCardInfo[] hiddenClientCards = GetHiddenOtherPlayersCards();
        foreach (ulong clientId in spectatingClients)
        {
            CardManager.Instance.DistributeCardInfoToPlayerClientRpc(
                sendingCards.ToArray(),
                hiddenClientCards,
                TurnManager.Instance.GetTurnOrderStartingAtClient(clientId).ToArray(),
                GetTotalCardsInPlay() <= CardManagerConstants.FlushLimit,
                new ClientRpcParams { Send = new() { TargetClientIds = new[] { clientId } } }
            );
        }
    }

    public void RevealAllCards()
    {
        foreach (var kvp in clientCards)
        {
            var clientId = kvp.Key;

            CardManager.Instance.RevealCardInfoToPlayerClientRpc(
                clientCards.Values.ToArray(),
                TurnManager.Instance.GetTurnOrderStartingAtClient(clientId).ToArray(),
                new ClientRpcParams { Send = new() { TargetClientIds = new[] { clientId } } }
            );
        }
    }

    public int GetTotalCardsInPlay() => clientCards.Values.Sum(i => i.amountOfCards);
}
