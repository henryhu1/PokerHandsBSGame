using System;
using Unity.Netcode;

[Serializable]
public class Card : INetworkSerializable, IComparable<Card>, IEquatable<Card>
{
    private Rank m_rank;
    public Rank Rank { get { return m_rank; } }
    private Suit m_suit;
    public Suit Suit {  get { return m_suit; } }

    public Card()
    {
        m_rank = Rank.Ace;
        m_suit = Suit.Spade;
    }

    public Card(Suit suit, Rank rank)
    {
        m_suit = suit;
        m_rank = rank;
        // TODO: validate rank, position, and rotation
        //string assetName = string.Format("Red_PlayingCards_{0}{1}_00", suit, rank);
        //GameObject asset = GameObject.Find(assetName);
        //if (asset == null)
        //{
        //    Debug.LogError("Asset '" + assetName + "' could not be found.");
        //}
        //else
        //    _card = Instantiate(asset, position, rotation);
        //}
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref m_rank);
        serializer.SerializeValue(ref m_suit);
    }

    public string GetCardName()
    {
        return $"{m_rank} of {m_suit}s";
    }

    public string GetCardIdentifier()
    {
        int rankIdentifier = (int)m_rank;
        if (m_rank == Rank.Ace)
        {
            rankIdentifier = 1;
        }
        return $"{m_suit}{rankIdentifier:D2}";
    }

    public int CompareTo(Card other)
    {
        int rankCompare = m_rank.CompareTo(other.m_rank);
        if (rankCompare == 0)
        {
            return m_suit.CompareTo(other.m_suit);
        }
        else
        {
            return rankCompare;
        }
    }

    public bool Equals(Card other)
    {
        return m_rank == other.m_rank && m_suit == other.m_suit;
    }
}
