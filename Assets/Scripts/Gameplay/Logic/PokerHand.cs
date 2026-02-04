using CardTraitExtensions;
using System;
using System.Linq;

[Serializable]
public abstract class PokerHand : IComparable<PokerHand> // UIMainScene.IUIInfoContent
{
    protected HandType handType;
    protected Rank rankPrimary;
    protected Rank rankSecondary;
    protected Suit suit;
    public abstract int RequiredCardsCount { get; }

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

    public virtual Suit[] GetSuitsForPokerHand()
    {
        return new Suit[0];
    }

    public virtual Rank[] GetRanksForPokerHand()
    {
        return new Rank[0];
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

    public PokerHandData ToNetworkData()
    {
        return new()
        {
            handType = handType,
            rankPrimary = rankPrimary,
            rankSecondary = rankSecondary,
            suit = suit,
        };
    }
}

public abstract class SingleRankHand : PokerHand
{
    public SingleRankHand() : base() { }

    public SingleRankHand(HandType hand, Rank rankPrimary) : base(hand)
    {
        this.rankPrimary = rankPrimary;
    }
}

public abstract class DoubleRankHand : PokerHand
{
    public DoubleRankHand() : base() { }

    public DoubleRankHand(HandType hand, Rank rankPrimary, Rank rankSecondary) : base(hand)
    {
        this.rankPrimary = rankPrimary;
        this.rankSecondary = rankSecondary;
    }
}

public abstract class RankSuitHand : PokerHand
{
    public RankSuitHand() : base() { }

    public RankSuitHand(HandType hand, Rank rankPrimary, Suit suit) : base(hand)
    {
        this.rankPrimary = rankPrimary;
        this.suit = suit;
    }
}

public abstract class SuitHand : PokerHand
{
    public SuitHand() : base() { }

    public SuitHand(HandType hand, Suit suit) : base(hand)
    {
        this.suit = suit;
    }
}

public class HighCard : SingleRankHand
{
    public override int RequiredCardsCount => 1;

    public HighCard() : base() { }

    public HighCard(PokerHand pokerHand) : this(pokerHand.GetPrimaryRank()) { }

    public HighCard(Rank rankPrimary) : base(HandType.HighCard, rankPrimary) { }

    public override Rank[] GetRanksForPokerHand()
    {
        return new Rank[] { rankPrimary };
    }

    public override string GetStringRepresentation()
    {
        return base.GetStringRepresentation() + $" {rankPrimary}";
    }
}

public class Pair : SingleRankHand
{
    public override int RequiredCardsCount => 2;

    public Pair() : base() { }

    public Pair(PokerHand pokerHand) : this(pokerHand.GetPrimaryRank()) { }

    public Pair(Rank rankPrimary) : base(HandType.Pair, rankPrimary) { }

    public override Rank[] GetRanksForPokerHand()
    {
        return new Rank[] { rankPrimary, rankPrimary };
    }

    public override string GetStringRepresentation()
    {
        return base.GetStringRepresentation() + $" of {rankPrimary}";
    }
}

public class TwoPair : DoubleRankHand
{
    public override int RequiredCardsCount => 4;

    public TwoPair() : base() { }

    public TwoPair(PokerHand pokerHand) : this(pokerHand.GetPrimaryRank(), pokerHand.GetSecondaryRank()) { }

    public TwoPair(Pair pair1, Pair pair2) : this(pair1.GetPrimaryRank(), pair2.GetPrimaryRank()) { }

    public TwoPair(Rank rankPrimary, Rank rankSecondary) : base(HandType.TwoPair, rankPrimary, rankSecondary) { }

    public override Rank[] GetRanksForPokerHand()
    {
        return new Rank[] { rankPrimary, rankPrimary, rankSecondary, rankSecondary };
    }

    public override string GetStringRepresentation()
    {
        return base.GetStringRepresentation() + $" {rankPrimary} {rankSecondary}";
    }
}

public class ThreeOfAKind : SingleRankHand
{
    public override int RequiredCardsCount => 3;

    public ThreeOfAKind() : base() { }

    public ThreeOfAKind(PokerHand pokerHand) : this(pokerHand.GetPrimaryRank()) { }

    public ThreeOfAKind(Rank rankPrimary) : base(HandType.ThreeOfAKind, rankPrimary) { }

    public override Rank[] GetRanksForPokerHand()
    {
        return new Rank[] { rankPrimary, rankPrimary, rankPrimary };
    }

    public override string GetStringRepresentation()
    {
        return base.GetStringRepresentation() + $" {rankPrimary}";
    }
}

public class Straight : SingleRankHand
{
    public static Rank s_LowestStraight = Rank.Five;

    public override int RequiredCardsCount => 5;

    public Straight() : base() { }

    public Straight(PokerHand pokerHand) : this(pokerHand.GetPrimaryRank()) { }

    public Straight(Rank rankPrimary) : base(HandType.Straight, rankPrimary) { }

    public override Rank[] GetRanksForPokerHand()
    {
        return rankPrimary.GetStraight().OrderByDescending(r => r).ToArray();
    }

    public override string GetStringRepresentation()
    {
        return base.GetStringRepresentation() + $" to {rankPrimary}";
    }
}

public class Flush : RankSuitHand
{
    public static Rank s_LowestFlush = Rank.Six;

    public override int RequiredCardsCount => 5;

    public Flush() : base() { }

    public Flush(PokerHand pokerHand) : this(pokerHand.GetPrimaryRank(), pokerHand.GetSuit()) { }

    public Flush(Rank rankPrimary, Suit suit) : base(HandType.Flush, rankPrimary, suit) { }

    public override Rank[] GetRanksForPokerHand()
    {
        return new Rank[] { rankPrimary };
    }

    public override Suit[] GetSuitsForPokerHand()
    {
        var suitArray = new Suit[5];
        Array.Fill(suitArray, suit);
        return suitArray;
    }

    public override string GetStringRepresentation()
    {
        return base.GetStringRepresentation() + $" {suit} high card {rankPrimary}";
    }
}

public class FullHouse : DoubleRankHand
{
    public override int RequiredCardsCount => 5;

    public FullHouse() : base() { }

    public FullHouse(PokerHand pokerHand) : this(pokerHand.GetPrimaryRank(), pokerHand.GetSecondaryRank()) { }

    public FullHouse(ThreeOfAKind triple, Pair pair) : this(triple.GetPrimaryRank(), pair.GetPrimaryRank()) { }

    public FullHouse(Rank rankPrimary, Rank rankSecondary) : base(HandType.FullHouse, rankPrimary, rankSecondary) { }

    public override Rank[] GetRanksForPokerHand()
    {
        return new Rank[] { rankPrimary, rankPrimary, rankPrimary, rankSecondary, rankSecondary };
    }

    public override string GetStringRepresentation()
    {
        return base.GetStringRepresentation() + $" {rankPrimary} over {rankSecondary}";
    }
}

public class FourOfAKind : SingleRankHand
{
    public override int RequiredCardsCount => 4;

    public FourOfAKind() : base() { }

    public FourOfAKind(PokerHand pokerHand) : this(pokerHand.GetPrimaryRank()) { }

    public FourOfAKind(Rank rankPrimary) : base(HandType.FourOfAKind, rankPrimary) { }

    public override Suit[] GetSuitsForPokerHand()
    {
        return new Suit[] { Suit.Club, Suit.Diamond, Suit.Heart, Suit.Spade };
    }

    public override Rank[] GetRanksForPokerHand()
    {
        return new Rank[] { rankPrimary, rankPrimary, rankPrimary, rankPrimary };
    }

    public override string GetStringRepresentation()
    {
        return base.GetStringRepresentation() + $" {rankPrimary}";
    }
}

public class StraightFlush : RankSuitHand
{
    public override int RequiredCardsCount => 5;

    public static Rank s_LowestStraightFlush = Rank.Five;

    public StraightFlush() : base() { }

    public StraightFlush(PokerHand pokerHand) : this(pokerHand.GetPrimaryRank(), pokerHand.GetSuit()) { }

    public StraightFlush(Rank rankPrimary, Suit suit) : base(HandType.StraightFlush, rankPrimary, suit) { }

    public override Suit[] GetSuitsForPokerHand()
    {
        var suitArray = new Suit[5];
        Array.Fill(suitArray, suit);
        return suitArray;
    }

    public override Rank[] GetRanksForPokerHand()
    {
        return rankPrimary.GetStraight().OrderByDescending(r => r).ToArray();
    }

    public override string GetStringRepresentation()
    {
        return base.GetStringRepresentation() + $" {suit} to {rankPrimary}";
    }
}

public class RoyalFlush : SuitHand
{
    public override int RequiredCardsCount => 5;

    public RoyalFlush() : base() { }

    public RoyalFlush(PokerHand pokerHand) : this(pokerHand.GetSuit()) { }

    public RoyalFlush(Suit suit) : base(HandType.RoyalFlush, suit) { }

    public override Suit[] GetSuitsForPokerHand()
    {
        var suitArray = new Suit[5];
        Array.Fill(suitArray, suit);
        return suitArray;
    }

    public override Rank[] GetRanksForPokerHand()
    {
        return new Rank[] { Rank.Ace, Rank.King, Rank.Queen, Rank.Jack, Rank.Ten };
    }

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
