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

    public void Reset()
    {
        SetColor(whiteColor);
        rankText.text = "";
        suitText.text = "";
    }

    public void DisplayRank(Rank rank)
    {
        rankText.text = ((int) rank).ToString();
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

    private void SetColor(Color color)
    {
        rankText.color = color;
        suitText.color = color;
    }
}
