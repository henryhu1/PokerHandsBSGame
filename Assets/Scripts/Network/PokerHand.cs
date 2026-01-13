using CardTraitExtensions;
using System;
using Unity.Netcode;

[Serializable]
public class PokerHand : INetworkSerializable, IComparable<PokerHand> // UIMainScene.IUIInfoContent
{
    protected HandType handType;
    protected Rank rankPrimary;
    protected Rank rankSecondary;
    protected Suit suit;

    public PokerHand() { }

    protected PokerHand(HandType handType)
    {
        this.handType = handType;
    }

    public virtual string GetStringRepresentation()
    {
        return handType.GetReadableHandString();
    }

    public int GetHandStrength()
    {
        return (int)handType;
    }

    public HandType GetHandType() { return handType; }

    public Rank GetPrimaryRank() { return rankPrimary; }

    public Rank GetSecondaryRank() { return rankSecondary; }

    public Suit GetSuit() { return suit; }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref handType);
        serializer.SerializeValue(ref rankPrimary);
        serializer.SerializeValue(ref rankSecondary);
        serializer.SerializeValue(ref suit);
    }

    public override string ToString()
    {
        return $"{handType} {rankPrimary} {rankSecondary} {suit}";
    }

    public int CompareTo(PokerHand other)
    {
        if (this < other) return -1;
        else if (this > other) return 1;
        else return 0;
    }

    public bool Equals(PokerHand other)
    {
        return handType == other.handType &&
            rankPrimary == other.rankPrimary &&
            rankSecondary == other.rankSecondary &&
            suit == other.suit;
    }

    public static bool operator <(PokerHand left, PokerHand right)
    {
        if (left.handType == right.handType)
        {
            if (left.rankPrimary == right.rankPrimary)
            {
                if (left.rankSecondary == right.rankSecondary)
                {
                    return false;
                }
                return left.rankSecondary < right.rankSecondary;
            }
            return left.rankPrimary < right.rankPrimary;
        }
        return left.handType < right.handType;
    }

    public static bool operator >(PokerHand left, PokerHand right)
    {
        if (left.handType == right.handType)
        {
            if (left.rankPrimary == right.rankPrimary)
            {
                if (left.rankSecondary == right.rankSecondary)
                {
                    return false;
                }
                return left.rankSecondary > right.rankSecondary;
            }
            return left.rankPrimary > right.rankPrimary;
        }
        return left.handType > right.handType;
    }
}

public class SingleRankHand : PokerHand
{
    public SingleRankHand() : base() { }

    public SingleRankHand(HandType hand, Rank rankPrimary) : base(hand)
    {
        this.rankPrimary = rankPrimary;
    }
}

public class DoubleRankHand : PokerHand
{
    public DoubleRankHand() : base() { }

    public DoubleRankHand(HandType hand, Rank rankPrimary, Rank rankSecondary) : base(hand)
    {
        this.rankPrimary = rankPrimary;
        this.rankSecondary = rankSecondary;
    }
}

public class RankSuitHand : PokerHand
{
    public RankSuitHand() : base() { }

    public RankSuitHand(HandType hand, Rank rankPrimary, Suit suit) : base(hand)
    {
        this.rankPrimary = rankPrimary;
        this.suit = suit;
    }
}

public class SuitHand : PokerHand
{
    public SuitHand() : base() { }

    public SuitHand(HandType hand, Suit suit) : base(hand)
    {
        this.suit = suit;
    }
}

public class HighCard : SingleRankHand
{
    public HighCard() : base() { }

    public HighCard(Rank rankPrimary) : base(HandType.HighCard, rankPrimary) { }

    public HighCard(PokerHand pokerHand) : base(HandType.HighCard, pokerHand.GetPrimaryRank()) { }

    public override string GetStringRepresentation()
    {
        return base.GetStringRepresentation() + $" {rankPrimary}";
    }
}

public class Pair : SingleRankHand
{
    public Pair() : base() { }

    public Pair(Rank rankPrimary) : base(HandType.Pair, rankPrimary) { }

    public Pair(PokerHand pokerHand) : base(HandType.Pair, pokerHand.GetPrimaryRank()) { }

    public override string GetStringRepresentation()
    {
        return base.GetStringRepresentation() + $" of {rankPrimary}";
    }
}

public class TwoPair : DoubleRankHand
{
    public TwoPair() : base() { }

    public TwoPair(Rank rankPrimary, Rank rankSecondary) : base(HandType.TwoPair, rankPrimary, rankSecondary) { }

    public TwoPair(PokerHand pokerHand) : base(HandType.TwoPair, pokerHand.GetPrimaryRank(), pokerHand.GetSecondaryRank()) { }

    public TwoPair(Pair pair1, Pair pair2) : base(HandType.TwoPair, pair1.GetPrimaryRank(), pair2.GetPrimaryRank()) { }

    public override string GetStringRepresentation()
    {
        return base.GetStringRepresentation() + $" {rankPrimary} {rankSecondary}";
    }
}

public class ThreeOfAKind : SingleRankHand
{
    public ThreeOfAKind() : base() { }

    public ThreeOfAKind(Rank rankPrimary) : base(HandType.ThreeOfAKind, rankPrimary) { }

    public ThreeOfAKind(PokerHand pokerHand) : base(HandType.ThreeOfAKind, pokerHand.GetPrimaryRank()) { }

    public override string GetStringRepresentation()
    {
        return base.GetStringRepresentation() + $" {rankPrimary}";
    }
}

public class Straight : SingleRankHand
{
    public static Rank s_LowestStraight = Rank.Five;

    public Straight() : base() { }

    public Straight(Rank rankPrimary) : base(HandType.Straight, rankPrimary) { }

    public Straight(PokerHand pokerHand) : base(HandType.Straight, pokerHand.GetPrimaryRank()) { }

    public override string GetStringRepresentation()
    {
        return base.GetStringRepresentation() + $" to {rankPrimary}";
    }
}

public class Flush : RankSuitHand
{
    public static Rank s_LowestFlush = Rank.Six;

    public Flush() : base() { }

    public Flush(Rank rankPrimary, Suit suit) : base(HandType.Flush, rankPrimary, suit) { }

    public Flush(PokerHand pokerHand) : base(HandType.Flush, pokerHand.GetPrimaryRank(), pokerHand.GetSuit()) { }

    public override string GetStringRepresentation()
    {
        return base.GetStringRepresentation() + $" {suit} high card {rankPrimary}";
    }
}

public class FullHouse : DoubleRankHand
{
    public FullHouse() : base() { }

    public FullHouse(Rank rankPrimary, Rank rankSecondary) : base(HandType.FullHouse, rankPrimary, rankSecondary) { }

    public FullHouse(PokerHand pokerHand) : base(HandType.FullHouse, pokerHand.GetPrimaryRank(), pokerHand.GetSecondaryRank()) { }

    public FullHouse(ThreeOfAKind triple, Pair pair) : base(HandType.FullHouse, triple.GetPrimaryRank(), pair.GetPrimaryRank()) { }

    public override string GetStringRepresentation()
    {
        return base.GetStringRepresentation() + $" {rankPrimary} over {rankSecondary}";
    }
}

public class FourOfAKind : SingleRankHand
{
    public FourOfAKind() : base() { }

    public FourOfAKind(Rank rankPrimary) : base(HandType.FourOfAKind, rankPrimary) { }

    public FourOfAKind(PokerHand pokerHand) : base(HandType.FourOfAKind, pokerHand.GetPrimaryRank()) { }

    public override string GetStringRepresentation()
    {
        return base.GetStringRepresentation() + $" {rankPrimary}";
    }
}

public class StraightFlush : RankSuitHand
{
    public static Rank s_LowestStraightFlush = Rank.Five;

    public StraightFlush() : base() { }

    public StraightFlush(Rank rankPrimary, Suit suit) : base(HandType.StraightFlush, rankPrimary, suit) { }

    public StraightFlush(PokerHand pokerHand) : base(HandType.StraightFlush, pokerHand.GetPrimaryRank(), pokerHand.GetSuit()) { }

    public override string GetStringRepresentation()
    {
        return base.GetStringRepresentation() + $" {suit} to {rankPrimary}";
    }
}

public class RoyalFlush : SuitHand
{
    public RoyalFlush() : base() { }

    public RoyalFlush(Suit suit) : base(HandType.RoyalFlush, suit) { }

    public RoyalFlush(PokerHand pokerHand) : base(HandType.RoyalFlush, pokerHand.GetSuit()) { }

    public override string GetStringRepresentation()
    {
        return base.GetStringRepresentation() + $" {suit}";
    }

    public static bool operator <(RoyalFlush left, RoyalFlush right)
    {
        return false;
    }

    public static bool operator >(RoyalFlush left, RoyalFlush right)
    {
        return false;
    }

}
