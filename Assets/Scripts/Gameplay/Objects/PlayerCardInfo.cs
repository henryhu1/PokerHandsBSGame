using System.Collections.Generic;
using Unity.Netcode;

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
        if (!serializer.IsWriter && cards == null)
            cards = new List<Card>();

        int count = cards?.Count ?? 0;
        serializer.SerializeValue(ref count);
        serializer.SerializeValue(ref amountOfCards);
        serializer.SerializeValue(ref playerName);

        if (serializer.IsWriter)
        {
            for (int i = 0; i < count; i++)
            {
                Card card = cards[i];
                serializer.SerializeValue(ref card);
            }
        }
        else
        {
            cards.Clear();
            for (int i = 0; i < count; i++)
            {
                Card card = new();
                serializer.SerializeValue(ref card);
                cards.Add(card);
            }
        }
    }
}
