using CardTraitExtensions;
using System.Collections.Generic;
using System.Linq;

public class HandsInPlay
{
    private int m_numberOfCardsInPlay;
    private Dictionary<Rank, uint> m_rankCount;
    private Dictionary<Suit, Dictionary<Rank, uint>> m_bySuitRankCount;
    private SortedSet<PokerHand> m_handsInPlay;

    public HandsInPlay()
    {
        ResetHandsInPlay();
    }

    private Dictionary<Rank, uint> GetEmptyDictionary()
    {
        return new Dictionary<Rank, uint> {
            { Rank.Two, 0 },
            { Rank.Three, 0 },
            { Rank.Four, 0 },
            { Rank.Five, 0 },
            { Rank.Six, 0 },
            { Rank.Seven, 0 },
            { Rank.Eight, 0 },
            { Rank.Nine, 0 },
            { Rank.Ten, 0 },
            { Rank.Jack, 0 },
            { Rank.Queen, 0 },
            { Rank.King, 0 },
            { Rank.Ace, 0 },
        };
    }

    public void ResetHandsInPlay()
    {
        m_numberOfCardsInPlay = 0;
        m_rankCount = GetEmptyDictionary();
        m_bySuitRankCount = new Dictionary<Suit, Dictionary<Rank, uint>>
        {
            { Suit.Spade, GetEmptyDictionary() },
            { Suit.Heart, GetEmptyDictionary() },
            { Suit.Diamond, GetEmptyDictionary() },
            { Suit.Club, GetEmptyDictionary() },
        };
        m_handsInPlay = new SortedSet<PokerHand>();
    }

    public void PopulateCardsInPlay(IEnumerable<Card> allCards)
    {
        foreach (Card card in allCards)
        {
            AddInPlayCard(card);
        }
    }

    public void AddInPlayCard(Card card)
    {
        m_numberOfCardsInPlay++;
        m_rankCount[card.Rank]++;
        m_bySuitRankCount[card.Suit][card.Rank]++;
    }

    private bool CheckRanks(List<Rank> ranks, Dictionary<Rank, uint> rankCounts)
    {
        foreach (Rank rank in ranks)
        {
            if (rankCounts[rank] == 0)
            {
                return false;
            }
        }
        return true;
    }

    private bool AreRanksInPlay(List<Rank> ranks)
    {
        return CheckRanks(ranks, m_rankCount);
    }
    
    private bool AreSuitedRanksInPlay(List<Rank> ranks, Suit suit)
    {
        return CheckRanks(ranks, m_bySuitRankCount[suit]);
    }

    public void FindHandsInPlay()
    {
        SortedSet<Pair> pairsInPlay = new SortedSet<Pair>();
        SortedSet<ThreeOfAKind> triplesInPlay = new SortedSet<ThreeOfAKind>();
        foreach (KeyValuePair<Rank, uint> rankCount in m_rankCount)
        {
            if (rankCount.Value >= 1)
            {
                m_handsInPlay.Add(new HighCard(rankCount.Key));
            }
            if (rankCount.Value >= 2)
            {
                Pair pairInPlay = new Pair(rankCount.Key);
                m_handsInPlay.Add(pairInPlay);
                pairsInPlay.Add(pairInPlay);
            }
            if (rankCount.Value >= 3)
            {
                ThreeOfAKind tripleInPlay = new ThreeOfAKind(rankCount.Key);
                m_handsInPlay.Add(tripleInPlay);
                triplesInPlay.Add(tripleInPlay);
            }
            if (rankCount.Value >= 4)
            {
                m_handsInPlay.Add(new FourOfAKind(rankCount.Key));
            }
        }

        for (int i = 0; i < pairsInPlay.Count; i++)
        {
            for (int j = i + 1; j < pairsInPlay.Count; j++)
            {
                m_handsInPlay.Add(new TwoPair(pairsInPlay.ElementAt(j), pairsInPlay.ElementAt(i)));
            }
            for (int  k = 0; k < triplesInPlay.Count; k++)
            {
                if (triplesInPlay.ElementAt(k).GetPrimaryRank() != pairsInPlay.ElementAt(i).GetPrimaryRank())
                {
                    m_handsInPlay.Add(new FullHouse(triplesInPlay.ElementAt(k), pairsInPlay.ElementAt(i)));
                }
            }
        }

        for (Rank highestInStraight = Straight.s_LowestStraight; highestInStraight <= Rank.Ace; highestInStraight += 1)
        {
            List<Rank> straight = highestInStraight.GetStraight();
            if (AreRanksInPlay(straight))
            {
                m_handsInPlay.Add(new Straight(highestInStraight));
            }
        }

        foreach (KeyValuePair<Suit, Dictionary<Rank, uint>> suitRanks in m_bySuitRankCount)
        {
            Suit checkingSuit = suitRanks.Key;

            if (m_numberOfCardsInPlay <= 15)
            {
                Dictionary<Rank, uint> ranksInPlay = suitRanks.Value;
                uint count = 0;
                foreach (KeyValuePair<Rank, uint> rankInPlay in ranksInPlay)
                {
                    if (rankInPlay.Value > 0)
                    {
                        count++;
                        if (count >= 5)
                        {
                            m_handsInPlay.Add(new Flush(rankInPlay.Key, checkingSuit));
                        }
                    }
                }
            }

            for (Rank highestInStraight = Straight.s_LowestStraight; highestInStraight <= Rank.King; highestInStraight += 1)
            {
                List<Rank> straight = highestInStraight.GetStraight();
                if (AreSuitedRanksInPlay(straight, checkingSuit))
                {
                    m_handsInPlay.Add(new StraightFlush(highestInStraight, checkingSuit));
                }
            }

            List<Rank> royalFlush = Rank.Ace.GetStraight();
            if (AreSuitedRanksInPlay(royalFlush, checkingSuit))
            {
                m_handsInPlay.Add(new RoyalFlush(checkingSuit));
            }
        }

        //StringBuilder stringBuilder = new StringBuilder();
        //stringBuilder.AppendLine("Hands in play:");
        //for (int i = 0; i < m_handsInPlay.Count; i++)
        //{
        //    stringBuilder.Append($"{m_handsInPlay.ElementAt(i).GetStringRepresentation()}");
        //    if (i < m_handsInPlay.Count - 1)
        //    {
        //        stringBuilder.Append(", ");
        //    }
        //}
        //Debug.Log(stringBuilder.ToString());
    }

    public bool IsHandInPlay(PokerHand pokerHand)
    {
        return m_handsInPlay.Contains(pokerHand);
    }

    public List<PokerHand> GetHandsInPlay()
    {
        return m_handsInPlay.ToList();
    }
}
