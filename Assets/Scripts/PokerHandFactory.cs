public static class PokerHandFactory
{
    public static PokerHand CreatePokerHand(HandType hand, Rank? primaryRank, Rank? secondaryRank, Suit? suit)
    {
        switch (hand)
        {
            case HandType.HighCard:
                return new HighCard((Rank)primaryRank);
            case HandType.Pair:
                return new Pair((Rank)primaryRank);
            case HandType.TwoPair:
                if ((Rank)primaryRank < (Rank)secondaryRank)
                {
                    return new TwoPair((Rank)secondaryRank, (Rank)primaryRank);
                }
                else
                {
                    return new TwoPair((Rank)primaryRank, (Rank)secondaryRank);
                }
            case HandType.ThreeOfAKind:
                return new ThreeOfAKind((Rank)primaryRank);
            case HandType.Straight:
                return new Straight((Rank)primaryRank);
            case HandType.Flush:
                return new Flush((Rank)primaryRank, (Suit)suit);
            case HandType.FullHouse:
                return new FullHouse((Rank)primaryRank, (Rank)secondaryRank);
            case HandType.FourOfAKind:
                return new FourOfAKind((Rank)primaryRank);
            case HandType.StraightFlush:
                return new StraightFlush((Rank)primaryRank, (Suit)suit);
            case HandType.RoyalFlush:
                return new RoyalFlush((Suit)suit);
            default:
                return null;
        }
    }

    public static PokerHand InferPokerHandType(PokerHandData pokerHand)
    {
        return pokerHand.handType switch
        {
            HandType.HighCard => new HighCard(pokerHand.rankPrimary),
            HandType.Pair => new Pair(pokerHand.rankPrimary),
            HandType.TwoPair => new TwoPair(pokerHand.rankPrimary, pokerHand.rankSecondary),
            HandType.ThreeOfAKind => new ThreeOfAKind(pokerHand.rankPrimary),
            HandType.Straight => new Straight(pokerHand.rankPrimary),
            HandType.Flush => new Flush(pokerHand.rankPrimary, pokerHand.suit),
            HandType.FullHouse => new FullHouse(pokerHand.rankPrimary, pokerHand.rankSecondary),
            HandType.FourOfAKind => new FourOfAKind(pokerHand.rankPrimary),
            HandType.StraightFlush => new StraightFlush(pokerHand.rankPrimary, pokerHand.suit),
            HandType.RoyalFlush => new RoyalFlush(pokerHand.suit),
            _ => null,
        };
    }
}
