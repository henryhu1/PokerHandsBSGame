using System.Collections.Generic;

public enum Suit
{
    Club = 1,
    Diamond = 2,
    Heart = 3,
    Spade = 4,
}

public enum Rank
{
    Two = 2,
    Three = 3,
    Four = 4,
    Five = 5,
    Six = 6,
    Seven = 7,
    Eight = 8,
    Nine = 9,
    Ten = 10,
    Jack = 11,
    Queen = 12,
    King = 13,
    Ace = 14,
}

public enum Hand
{
    HighCard = 1,
    Pair = 2,
    TwoPair = 3,
    ThreeOfAKind = 4,
    Straight = 5,
    Flush = 6,
    FullHouse = 7,
    FourOfAKind = 8,
    StraightFlush = 9,
    RoyalFlush = 10,
}

namespace CardTraitExtensions
{
    public static class CardTraitExtensions
    {
        private const string KEY_PRIMARY_RANK = "primary_rank";
        private const string KEY_SECONDARY_RANK = "secondary_rank";
        private const string KEY_SUIT = "suit";
        private static Dictionary<string, bool> s_OneRankSelections = new Dictionary<string, bool>
        {
            { KEY_PRIMARY_RANK, true },
            { KEY_SECONDARY_RANK, false },
            { KEY_SUIT, false },
        };

        private static Dictionary<string, bool> s_TwoRanksSelections = new Dictionary<string, bool>
        {
            { KEY_PRIMARY_RANK, true },
            { KEY_SECONDARY_RANK, true },
            { KEY_SUIT, false },
        };

        private static Dictionary<string, bool> s_SuitSelections = new Dictionary<string, bool>
        {
            { KEY_PRIMARY_RANK, false },
            { KEY_SECONDARY_RANK, false },
            { KEY_SUIT, true },
        };

        private static Dictionary<string, bool> s_RankSuitSelections = new Dictionary<string, bool>
        {
            { KEY_PRIMARY_RANK, true },
            { KEY_SECONDARY_RANK, false },
            { KEY_SUIT, true },
        };

        private static Dictionary<Hand, Dictionary<string, bool>> s_HandsRequiredSelections = new Dictionary<Hand, Dictionary<string, bool>>
        {
            { Hand.HighCard, s_OneRankSelections },
            { Hand.Pair, s_OneRankSelections },
            { Hand.TwoPair, s_TwoRanksSelections },
            { Hand.ThreeOfAKind, s_OneRankSelections },
            { Hand.Straight, s_OneRankSelections },
            { Hand.Flush, s_RankSuitSelections },
            { Hand.FullHouse, s_TwoRanksSelections },
            { Hand.FourOfAKind, s_OneRankSelections },
            { Hand.StraightFlush, s_OneRankSelections },
            { Hand.RoyalFlush, s_SuitSelections },
        };

        public static bool IsSelectionCorrect(this Hand hand, Rank? primaryRank, Rank? secondaryRank, Suit? suit)
        {
            Dictionary<string, bool> requiredSelections = s_HandsRequiredSelections[hand];
            bool isPrimaryRankSelected = primaryRank != null;
            bool isSecondaryRankSelected = secondaryRank != null;
            bool isSuitSelected = suit != null;
            return (!requiredSelections[KEY_PRIMARY_RANK] || isPrimaryRankSelected) &&
                (!requiredSelections[KEY_SECONDARY_RANK] || isSecondaryRankSelected) &&
                (!requiredSelections[KEY_SUIT] || isSuitSelected);
        }
        
        public static List<Rank> GetStraight(this Rank rankHighestInStraight)
        {
            if (rankHighestInStraight < Straight.s_LowestStraight)
            {
                throw new System.Exception($"Straight to {rankHighestInStraight} is not possible");
            }
            Rank rankLowestInStraight = rankHighestInStraight - 4;
            if (rankHighestInStraight == Straight.s_LowestStraight)
            {
                rankLowestInStraight = Rank.Ace;
            }
            return new List<Rank> { rankLowestInStraight, rankHighestInStraight - 3, rankHighestInStraight - 2, rankHighestInStraight - 1, rankHighestInStraight };
        }

        public static string GetReadableHandString(this Hand hand)
        {
            return StringUtils.SplitCapitals(hand.ToString());
        }
    }
}
