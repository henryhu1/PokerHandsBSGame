public static class PokerHandFactory
{
    public static PokerHand CreatePokerHand(Hand hand, Rank? primaryRank, Rank? secondaryRank, Suit? suit)
    {
        switch (hand)
        {
            case Hand.HighCard:
                return new HighCard((Rank)primaryRank);
            case Hand.Pair:
                return new Pair((Rank)primaryRank);
            case Hand.TwoPair:
                if ((Rank)primaryRank < (Rank)secondaryRank)
                {
                    return new TwoPair((Rank)secondaryRank, (Rank)primaryRank);
                }
                else
                {
                    return new TwoPair((Rank)primaryRank, (Rank)secondaryRank);
                }
            case Hand.ThreeOfAKind:
                return new ThreeOfAKind((Rank)primaryRank);
            case Hand.Straight:
                return new Straight((Rank)primaryRank);
            case Hand.Flush:
                return new Flush((Rank)primaryRank, (Suit)suit);
            case Hand.FullHouse:
                return new FullHouse((Rank)primaryRank, (Rank)secondaryRank);
            case Hand.FourOfAKind:
                return new FourOfAKind((Rank)primaryRank);
            case Hand.StraightFlush:
                return new StraightFlush((Rank)primaryRank, (Suit)suit);
            case Hand.RoyalFlush:
                return new RoyalFlush((Suit)suit);
            default:
                return null;
        }
    }

    public static PokerHand InferPokerHandType(PokerHand pokerHand)
    {
        switch (pokerHand.Hand)
        {
            case Hand.HighCard:
                return new HighCard(pokerHand);
            case Hand.Pair:
                return new Pair(pokerHand);
            case Hand.TwoPair:
                return new TwoPair(pokerHand);
            case Hand.ThreeOfAKind:
                return new ThreeOfAKind(pokerHand);
            case Hand.Straight:
                return new Straight(pokerHand);
            case Hand.Flush:
                return new Flush(pokerHand);
            case Hand.FullHouse:
                return new FullHouse(pokerHand);
            case Hand.FourOfAKind:
                return new FourOfAKind(pokerHand);
            case Hand.StraightFlush:
                return new StraightFlush(pokerHand);
            case Hand.RoyalFlush:
                return new RoyalFlush(pokerHand);
            default:
                return null;
        }
    }
}
