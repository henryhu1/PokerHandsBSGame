using CardTraitExtensions;
using TMPro;
using UnityEngine;

public class SmallCardUIController : MonoBehaviour
{
    private readonly Color redColor = new(1, 0.513f, 0.513f, 1);
    private readonly Color blackColor = Color.black;
    private readonly Color whiteColor = Color.white;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI rankText;
    [SerializeField] private TextMeshProUGUI suitText;

    private void OnDisable()
    {
        Reset();
    }

    private void Reset()
    {
        rankText.text = "";
        ResetSuit();
    }

    public void DisplayRank(Rank rank)
    {
        rankText.text = rank.GetRankSymbol();
    }

    public void DisplaySuit(Suit suit)
    {
        switch (suit)
        {
            case Suit.Heart:
                suitText.text = "♥";
                SetColor(redColor);
                break;
            case Suit.Diamond:
                suitText.text = "♦";
                SetColor(redColor);
                break;
            case Suit.Club:
                suitText.text = "♣";
                SetColor(blackColor);
                break;
            case Suit.Spade:
                suitText.text = "♠";
                SetColor(blackColor);
                break;
        }
    }

    public void ResetSuit()
    {
        SetColor(whiteColor);
        suitText.text = "";
    }

    private void SetColor(Color color)
    {
        rankText.color = color;
        suitText.color = color;
    }
}
