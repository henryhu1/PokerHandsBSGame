using UnityEngine;

public class HandPreviewUIController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject uiItem;
    [SerializeField] private SmallCardUIController[] smallCards;
    private RectTransform rectTransform;

    [Header("Listening Events")]
    [SerializeField] private PokerHandEventChannelSO OnPreviewPokerHand;
    [SerializeField] private VoidEventChannelSO OnStopPreview;

    private PokerHand handToPreview;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    private void Start()
    {
        Hide();
    }

    private void OnEnable()
    {
        OnPreviewPokerHand.OnEventRaised += SetHandToPreview;
        OnStopPreview.OnEventRaised += DelayHidePreview;
    }

    private void OnDisable()
    {
        OnPreviewPokerHand.OnEventRaised -= SetHandToPreview;
        OnStopPreview.OnEventRaised -= DelayHidePreview;
    }

    private void SetHandToPreview(PokerHand pokerHand)
    {
        CancelInvoke();

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
        }
    }

    private void DisplayCardPreviewRanks(Rank[] ranks)
    {
        for (int i = 0; i < ranks.Length; i++)
        {
            smallCards[i].DisplayRank(ranks[i]);
        }
        for (int i = ranks.Length; i < smallCards.Length; i++)
        {
            smallCards[i].ResetRank();
        }
    }

    private void DisplayCardPreviewSuits(Suit[] suits)
    {
        for (int i = 0; i < suits.Length; i++)
        {
            smallCards[i].DisplaySuit(suits[i]);
        }
        for (int i = suits.Length; i < smallCards.Length; i++)
        {
            smallCards[i].ResetSuit();
        }
    }

    private void DelayHidePreview()
    {
        Invoke(nameof(Hide), 0.25f);
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
