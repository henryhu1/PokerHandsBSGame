using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Card Registry")]
public class CardRegistrySO : ScriptableObject
{
    [System.Serializable]
    public class Entry
    {
        public CardSO card;
        public GameObject cardPrefab;
        public Material material;
    }

    public Entry[] mappings;

    public GameObject GetPrefab(Rank rank, Suit suit)
    {
        foreach (var m in mappings)
        {
            if (m.card.rank == rank && m.card.suit == suit)
                return m.cardPrefab;
        }
        return null;
    }

    public Entry GetEntry(Card card)
    {
        foreach (var m in mappings)
        {
            if (m.card.rank == card.Rank && m.card.suit == card.Suit)
                return m;
        }
        return null;
    }
}
