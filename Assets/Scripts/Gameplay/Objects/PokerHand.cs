using CardTraitExtensions;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;

[Serializable]
public class PokerHand : INetworkSerializable, IComparable<PokerHand> // UIMainScene.IUIInfoContent
{
    protected Hand m_hand;
    public Hand Hand
    {
        get { return m_hand; }
        set { m_hand = value; }
    }
    protected Rank m_rankPrimary;
    public Rank RankPrimary
    {
        get { return m_rankPrimary; }
        set { m_rankPrimary = value; }
    }
    protected Rank m_rankSecondary;
    public Rank RankSecondary
    {
        get { return m_rankSecondary; }
        set { m_rankSecondary = value; }
    }
    protected Suit m_suit;
    public Suit Suit
    {
        get { return m_suit; }
        set { m_suit = value; }
    }

    public PokerHand() { }

    protected PokerHand(Hand hand)
    {
        m_hand = hand;
    }

    public virtual string GetStringRepresentation()
    {
        return m_hand.GetReadableHandString();
    }

    public int GetHandStrength()
    {
        return (int)m_hand;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref m_hand);
        serializer.SerializeValue(ref m_rankPrimary);
        serializer.SerializeValue(ref m_rankSecondary);
        serializer.SerializeValue(ref m_suit);
    }

    public int CompareTo(PokerHand other)
    {
        if (this < other) return -1;
        else if (this > other) return 1;
        else return 0;
    }

    public static bool operator <(PokerHand left, PokerHand right)
    {
        if (left.m_hand == right.m_hand)
        {
            if (left.m_rankPrimary == right.m_rankPrimary)
            {
                if (left.m_rankSecondary == right.m_rankSecondary)
                {
                    return false;
                }
                return left.m_rankSecondary < right.m_rankSecondary;
            }
            return left.m_rankPrimary < right.m_rankPrimary;
        }
        return left.m_hand < right.m_hand;
    }

    public static bool operator >(PokerHand left, PokerHand right)
    {
        if (left.m_hand == right.m_hand)
        {
            if (left.m_rankPrimary == right.m_rankPrimary)
            {
                if (left.m_rankSecondary == right.m_rankSecondary)
                {
                    return false;
                }
                return left.m_rankSecondary > right.m_rankSecondary;
            }
            return left.m_rankPrimary > right.m_rankPrimary;
        }
        return left.m_hand > right.m_hand;
    }
}

public class SingleRankHand : PokerHand
{
    public SingleRankHand() : base() { }

    public SingleRankHand(Hand hand, Rank rankPrimary) : base(hand)
    {
        m_rankPrimary = rankPrimary;
    }
}

public class DoubleRankHand : PokerHand
{
    public DoubleRankHand() : base() { }

    public DoubleRankHand(Hand hand, Rank rankPrimary, Rank rankSecondary) : base(hand)
    {
        m_rankPrimary = rankPrimary;
        m_rankSecondary = rankSecondary;
    }
}

public class RankSuitHand : PokerHand
{
    public RankSuitHand() : base() { }

    public RankSuitHand(Hand hand, Rank rankPrimary, Suit suit) : base(hand)
    {
        m_rankPrimary = rankPrimary;
        m_suit = suit;
    }
}

public class SuitHand : PokerHand
{
    public SuitHand() : base() { }

    public SuitHand(Hand hand, Suit suit) : base(hand)
    {
        m_suit = suit;
    }
}

public class HighCard : SingleRankHand
{
    public HighCard() : base() { }

    public HighCard(Rank rankPrimary) : base(Hand.HighCard, rankPrimary) { }

    public HighCard(PokerHand pokerHand) : base(Hand.HighCard, pokerHand.RankPrimary) { }

    public override string GetStringRepresentation()
    {
        return base.GetStringRepresentation() + $" {m_rankPrimary}";
    }
}

public class Pair : SingleRankHand
{
    public Pair() : base() { }

    public Pair(Rank rankPrimary) : base(Hand.Pair, rankPrimary) { }

    public Pair(PokerHand pokerHand) : base(Hand.Pair, pokerHand.RankPrimary) { }

    public override string GetStringRepresentation()
    {
        return base.GetStringRepresentation() + $" of {m_rankPrimary}";
    }
}

public class TwoPair : DoubleRankHand
{
    public TwoPair() : base() { }

    public TwoPair(Rank rankPrimary, Rank rankSecondary) : base(Hand.TwoPair, rankPrimary, rankSecondary) { }

    public TwoPair(PokerHand pokerHand) : base(Hand.TwoPair, pokerHand.RankPrimary, pokerHand.RankSecondary) { }

    public TwoPair(Pair pair1, Pair pair2) : base(Hand.TwoPair, pair1.RankPrimary, pair2.RankPrimary) { }

    public override string GetStringRepresentation()
    {
        return base.GetStringRepresentation() + $" {m_rankPrimary} {m_rankSecondary}";
    }
}

public class ThreeOfAKind : SingleRankHand
{
    public ThreeOfAKind() : base() { }

    public ThreeOfAKind(Rank rankPrimary) : base(Hand.ThreeOfAKind, rankPrimary) { }

    public ThreeOfAKind(PokerHand pokerHand) : base(Hand.ThreeOfAKind, pokerHand.RankPrimary) { }

    public override string GetStringRepresentation()
    {
        return base.GetStringRepresentation() + $" {m_rankPrimary}";
    }
}

public class Straight : SingleRankHand
{
    public static Rank s_LowestStraight = Rank.Five;

    public Straight() : base() { }

    public Straight(Rank rankPrimary) : base(Hand.Straight, rankPrimary) { }

    public Straight(PokerHand pokerHand) : base(Hand.Straight, pokerHand.RankPrimary) { }

    public override string GetStringRepresentation()
    {
        return base.GetStringRepresentation() + $" to {m_rankPrimary}";
    }
}

public class Flush : RankSuitHand
{
    public static Rank s_LowestFlush = Rank.Six;

    public Flush() : base() { }

    public Flush(Rank rankPrimary, Suit suit) : base(Hand.Flush, rankPrimary, suit) { }

    public Flush(PokerHand pokerHand) : base(Hand.Flush, pokerHand.RankPrimary, pokerHand.Suit) { }

    public override string GetStringRepresentation()
    {
        return base.GetStringRepresentation() + $" {m_suit} high card {m_rankPrimary}";
    }
}

public class FullHouse : DoubleRankHand
{
    public FullHouse() : base() { }

    public FullHouse(Rank rankPrimary, Rank rankSecondary) : base(Hand.FullHouse, rankPrimary, rankSecondary) { }

    public FullHouse(PokerHand pokerHand) : base(Hand.FullHouse, pokerHand.RankPrimary, pokerHand.RankSecondary) { }

    public FullHouse(ThreeOfAKind triple, Pair pair) : base(Hand.FullHouse, triple.RankPrimary, pair.RankPrimary) { }

    public override string GetStringRepresentation()
    {
        return base.GetStringRepresentation() + $" {m_rankPrimary} over {m_rankSecondary}";
    }
}

public class FourOfAKind : SingleRankHand
{
    public FourOfAKind() : base() { }

    public FourOfAKind(Rank rankPrimary) : base(Hand.FourOfAKind, rankPrimary) { }

    public FourOfAKind(PokerHand pokerHand) : base(Hand.FourOfAKind, pokerHand.RankPrimary) { }

    public override string GetStringRepresentation()
    {
        return base.GetStringRepresentation() + $" {m_rankPrimary}";
    }
}

public class StraightFlush : RankSuitHand
{
    public static Rank s_LowestStraightFlush = Rank.Five;

    public StraightFlush() : base() { }

    public StraightFlush(Rank rankPrimary, Suit suit) : base(Hand.StraightFlush, rankPrimary, suit) { }

    public StraightFlush(PokerHand pokerHand) : base(Hand.StraightFlush, pokerHand.RankPrimary, pokerHand.Suit) { }

    public override string GetStringRepresentation()
    {
        return base.GetStringRepresentation() + $" {m_suit} to {m_rankPrimary}";
    }
}

public class RoyalFlush : SuitHand
{
    public RoyalFlush() : base() { }

    public RoyalFlush(Suit suit) : base(Hand.RoyalFlush, suit) { }

    public RoyalFlush(PokerHand pokerHand) : base(Hand.RoyalFlush, pokerHand.Suit) { }

    public override string GetStringRepresentation()
    {
        return base.GetStringRepresentation() + $" {m_suit}";
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
