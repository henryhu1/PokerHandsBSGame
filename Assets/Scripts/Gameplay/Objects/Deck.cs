using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

[Serializable]
public class Deck// : MonoBehaviour, UIMainScene.IUIInfoContent
{
    public List<Card> m_deck { get; private set; }

    public static readonly IList<Card> fullDeck = new ReadOnlyCollection<Card>(new List<Card> {
        new Card(Suit.Spade, Rank.Ace), new Card(Suit.Spade, Rank.Two), new Card(Suit.Spade, Rank.Three), new Card(Suit.Spade, Rank.Four), new Card(Suit.Spade, Rank.Five), new Card(Suit.Spade, Rank.Six), new Card(Suit.Spade, Rank.Seven), new Card(Suit.Spade, Rank.Eight), new Card(Suit.Spade, Rank.Nine), new Card(Suit.Spade, Rank.Ten), new Card(Suit.Spade, Rank.Jack), new Card(Suit.Spade, Rank.Queen), new Card(Suit.Spade, Rank.King), 
        new Card(Suit.Heart, Rank.Ace), new Card(Suit.Heart, Rank.Two), new Card(Suit.Heart, Rank.Three), new Card(Suit.Heart, Rank.Four), new Card(Suit.Heart, Rank.Five), new Card(Suit.Heart, Rank.Six), new Card(Suit.Heart, Rank.Seven), new Card(Suit.Heart, Rank.Eight), new Card(Suit.Heart, Rank.Nine), new Card(Suit.Heart, Rank.Ten), new Card(Suit.Heart, Rank.Jack), new Card(Suit.Heart, Rank.Queen), new Card(Suit.Heart, Rank.King), 
        new Card(Suit.Diamond, Rank.Ace), new Card(Suit.Diamond, Rank.Two), new Card(Suit.Diamond, Rank.Three), new Card(Suit.Diamond, Rank.Four), new Card(Suit.Diamond, Rank.Five), new Card(Suit.Diamond, Rank.Six), new Card(Suit.Diamond, Rank.Seven), new Card(Suit.Diamond, Rank.Eight), new Card(Suit.Diamond, Rank.Nine), new Card(Suit.Diamond, Rank.Ten), new Card(Suit.Diamond, Rank.Jack), new Card(Suit.Diamond, Rank.Queen), new Card(Suit.Diamond, Rank.King), 
        new Card(Suit.Club, Rank.Ace), new Card(Suit.Club, Rank.Two), new Card(Suit.Club, Rank.Three), new Card(Suit.Club, Rank.Four), new Card(Suit.Club, Rank.Five), new Card(Suit.Club, Rank.Six), new Card(Suit.Club, Rank.Seven), new Card(Suit.Club, Rank.Eight), new Card(Suit.Club, Rank.Nine), new Card(Suit.Club, Rank.Ten), new Card(Suit.Club, Rank.Jack), new Card(Suit.Club, Rank.Queen), new Card(Suit.Club, Rank.King), 
    });
    private System.Random _random = new System.Random();

    public Deck()
    {
        m_deck = new List<Card>(fullDeck);
    }

    public void Shuffle()
    {
        if (m_deck == null) return;

        var count = m_deck.Count;
        var last = count - 1;
        for (var i = 0; i < last; ++i)
        {
            var r = UnityEngine.Random.Range(i, count);
            var tmp = m_deck[i];
            m_deck[i] = m_deck[r];
            m_deck[r] = tmp;
        }
    }

    public bool IsDeckEmpty()
    {
        return m_deck.Count == 0;
    }

    public Card TakeCard()
    {
        if (IsDeckEmpty()) return null;

        Card card = m_deck[0];
        m_deck.RemoveAt(0);

        return card;
    }

    public void ResetDeck()
    {
        m_deck = new List<Card>(fullDeck);
    }
}
