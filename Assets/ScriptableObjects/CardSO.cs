using UnityEngine;

[CreateAssetMenu(fileName = "CardSO", menuName = "Scriptable Objects/CardSO")]
public class CardSO : ScriptableObject
{
    public Rank rank;
    public Suit suit;

    public string Name() => $"{rank} of ${suit}s";
}
