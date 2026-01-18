using UnityEngine;

public class HandPreviewUIController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject uiItem;
    [SerializeField] private SmallCardUIController[] smallCards;

    private PokerHand handToPreview;

    private void SetHandToPreview(PokerHand pokerHand)
    {
        handToPreview = pokerHand;
        DisplayCardPreviews(handToPreview.RequiredCardsCount);

        DisplayCardPreviewRanks(handToPreview.GetRanksForPokerHand());
        DisplayCardPreviewSuits(handToPreview.GetSuitsForPokerHand());

        Show();
    }

    private void DisplayCardPreviews(int count)
    {
        for (int i = 0; i < smallCards.Length; i++)
        {
            smallCards[i].gameObject.SetActive(i < count);
            smallCards[i].Reset();
        }
    }

    private void DisplayCardPreviewRanks(Rank[] ranks)
    {
        for (int i = 0; i < ranks.Length; i++)
        {
            smallCards[i].DisplayRank(ranks[i]);
        }
    }

    private void DisplayCardPreviewSuits(Suit[] suits)
    {
        for (int i = 0; i < suits.Length; i++)
        {
            smallCards[i].DisplaySuit(suits[i]);
        }
    }

    private void Show()
    {
        uiItem.SetActive(true);
    }

    private void Hide()
    {
        DisplayCardPreviews(0);
        uiItem.SetActive(false);
    }
}
