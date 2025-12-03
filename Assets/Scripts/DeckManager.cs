using System.Collections.Generic;

public class DeckManager
{
    private Deck deck;

    public DeckManager()
    {
        deck = new Deck();
    }

    public void ResetAndShuffle()
    {
        deck.ResetDeck();
        deck.Shuffle();
    }

    public Card DrawCard() => deck.TakeCard();

    public List<Card> GetAllCardsInDeck() => deck.m_deck;
}
