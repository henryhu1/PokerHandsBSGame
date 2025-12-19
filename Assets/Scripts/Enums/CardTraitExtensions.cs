using System.Collections.Generic;

namespace CardTraitExtensions
{
    public static class CardTraitExtensions
    {
        private const string KEY_PRIMARY_RANK = "primary_rank";
        private const string KEY_SECONDARY_RANK = "secondary_rank";
        private const string KEY_SUIT = "suit";
        private static Dictionary<string, bool> s_OneRankSelections = new()
        {
            { KEY_PRIMARY_RANK, true },
            { KEY_SECONDARY_RANK, false },
            { KEY_SUIT, false },
        };

        private static Dictionary<string, bool> s_TwoRanksSelections = new()
        {
            { KEY_PRIMARY_RANK, true },
            { KEY_SECONDARY_RANK, true },
            { KEY_SUIT, false },
        };

        private static Dictionary<string, bool> s_SuitSelections = new()
        {
            { KEY_PRIMARY_RANK, false },
            { KEY_SECONDARY_RANK, false },
            { KEY_SUIT, true },
        };

        private static Dictionary<string, bool> s_RankSuitSelections = new()
        {
            { KEY_PRIMARY_RANK, true },
            { KEY_SECONDARY_RANK, false },
            { KEY_SUIT, true },
        };

        private static Dictionary<HandType, Dictionary<string, bool>> s_HandsRequiredSelections = new()
        {
            { HandType.HighCard, s_OneRankSelections },
            { HandType.Pair, s_OneRankSelections },
            { HandType.TwoPair, s_TwoRanksSelections },
            { HandType.ThreeOfAKind, s_OneRankSelections },
            { HandType.Straight, s_OneRankSelections },
            { HandType.Flush, s_RankSuitSelections },
            { HandType.FullHouse, s_TwoRanksSelections },
            { HandType.FourOfAKind, s_OneRankSelections },
            { HandType.StraightFlush, s_OneRankSelections },
            { HandType.RoyalFlush, s_SuitSelections },
        };

        public static bool IsSelectionCorrect(this HandType hand, Rank? primaryRank, Rank? secondaryRank, Suit? suit)
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

        public static string GetReadableHandString(this HandType hand)
        {
            return StringUtils.SplitCapitals(hand.ToString());
        }
    }
}
