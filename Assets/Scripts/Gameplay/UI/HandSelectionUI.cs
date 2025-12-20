using CardTraitExtensions;
using TMPro;
using UnityEngine;

public class HandSelectionUI : MonoBehaviour
{
    // TODO: remove Singleton?
    public static HandSelectionUI Instance { get; private set; }

    [Header("UI")]
    [SerializeField] private CardRankChoicesUI cardRankChoicesPrimary;
    [SerializeField] private CardRankChoicesUI cardRankChoicesSecondary;
    [SerializeField] private CardSuitChoicesUI cardSuitChoices;
    [SerializeField] private TextMeshProUGUI byThreeText;
    [SerializeField] private TextMeshProUGUI byTwoText;

    [Header("Listening Events")]
    [SerializeField] private HandTypeEventChannelSO OnSelectHandType;
    [SerializeField] private RankEventChannelSO OnSelectPrimaryRank;
    [SerializeField] private RankEventChannelSO OnSelectSecondaryRank;
    [SerializeField] private VoidEventChannelSO OnNoSelectionHand;
    [SerializeField] private VoidEventChannelSO OnNoSelectionPrimaryRank;
    [SerializeField] private VoidEventChannelSO OnNoSelectionSecondaryRank;
    [SerializeField] private VoidEventChannelSO OnNoSelectionSuit;
    [SerializeField] private PokerHandEventChannelSO OnUpdatePlayableHands;

    // TODO (1): should probably move each field to be in their toggle script
    private Rank? selectedPrimaryRank;
    private Rank? selectedSecondaryRank;
    private Suit? selectedSuit;
    private HandType? selectedHand;

    private void Awake()
    {
        if (Instance != this && Instance != null)
        {
            Destroy(Instance.gameObject);
        }
        Instance = this;

        byThreeText.gameObject.SetActive(false);
        byTwoText.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        selectedHand = null;
        HideAllSelections();

        OnSelectPrimaryRank.OnEventRaised += SelectPrimaryRank;
        OnNoSelectionPrimaryRank.OnEventRaised += PrimaryRankChoice_NoSelectionMade;

        OnSelectSecondaryRank.OnEventRaised += SelectSecondaryRank;
        OnNoSelectionSecondaryRank.OnEventRaised += SecondaryRankChoice_NoSelectionMade;

        cardSuitChoices.OnSelectSuit += SuitChoice_SelectSuit;
        OnNoSelectionSuit.OnEventRaised += SuitChoice_NoSelectionMade;

        OnSelectHandType.OnEventRaised += HandleHandTypeSelection;
        OnNoSelectionHand.OnEventRaised += HandTypeUI_NoSelectionMade;

        OnUpdatePlayableHands.OnEventRaised += UpdatePlayableHands;
    }

    private void OnDisable()
    {
        OnSelectPrimaryRank.OnEventRaised -= SelectPrimaryRank;
        OnNoSelectionPrimaryRank.OnEventRaised -= PrimaryRankChoice_NoSelectionMade;

        OnSelectSecondaryRank.OnEventRaised -= SelectSecondaryRank;
        OnNoSelectionSecondaryRank.OnEventRaised -= SecondaryRankChoice_NoSelectionMade;

        cardSuitChoices.OnSelectSuit -= SuitChoice_SelectSuit;
        OnNoSelectionSuit.OnEventRaised -= SuitChoice_NoSelectionMade;

        OnSelectHandType.OnEventRaised -= HandleHandTypeSelection;
        OnNoSelectionHand.OnEventRaised -= HandTypeUI_NoSelectionMade;

        OnUpdatePlayableHands.OnEventRaised -= UpdatePlayableHands;
    }

    private void HandleHandTypeSelection(HandType newHandSelected)
    {
        HideAllSelections();
        selectedHand = newHandSelected;

        if (newHandSelected != HandType.FullHouse)
        {
            byThreeText.gameObject.SetActive(false);
            byTwoText.gameObject.SetActive(false);
        }

        switch (selectedHand)
        {
            case HandType.RoyalFlush:
                cardSuitChoices.Show(HandType.RoyalFlush);
                break;
            case HandType.TwoPair:
            case HandType.FullHouse:
                cardRankChoicesSecondary.Show(newHandSelected);
                cardRankChoicesPrimary.Show(newHandSelected);
                break;
            case HandType.Flush:
            case HandType.StraightFlush:
                cardSuitChoices.Show(newHandSelected);
                cardRankChoicesPrimary.Show(newHandSelected);
                break;
            default:
                cardRankChoicesPrimary.Show(newHandSelected);
                break;
        }
    }

    private void UpdatePlayableHands(PokerHand lastPlayedHand)
    {
        HandType lastPlayedHandType = lastPlayedHand.GetHandType();
        if (lastPlayedHandType == selectedHand)
        {
            HandleHandTypeSelection(lastPlayedHandType);
        }
        else if (lastPlayedHandType > selectedHand)
        {
            // TODO (1): this selection is cleared, but a HandTypeUI toggle may still be active
            //   Although since the rank/suit selection gets hidden
            //   a hand type must be selected anew so it should not be a problem
            selectedHand = null;
            HideAllSelections();
        }
    }

    private void HandTypeUI_NoSelectionMade()
    {
        selectedHand = null;
        HideAllSelections();
    }

    private void SelectPrimaryRank(Rank rank)
    {
        RankChoice_SelectRank(true, rank);
    }

    private void SelectSecondaryRank(Rank rank)
    {
        RankChoice_SelectRank(false, rank);
    }

    private void RankChoice_SelectRank(bool isPrimary, Rank selectedRank)
    {
#if UNITY_EDITOR
        Debug.Log($"rank selected, primary={isPrimary} rank={selectedRank}");
#endif
        bool isTwoPairSelection = selectedHand != null && selectedHand.Equals(HandType.TwoPair);

        if (selectedHand.Equals(HandType.FullHouse))
        {
            if (isPrimary)
            {
                byThreeText.gameObject.SetActive(true);
                Vector3 togglePosition = cardRankChoicesPrimary.GetTogglePosition(selectedRank);
                Vector3 textPosition = byThreeText.transform.position;
                byThreeText.transform.position = new Vector3(textPosition.x, togglePosition.y, textPosition.z);
            }
            else
            {
                byTwoText.gameObject.SetActive(true);
                Vector3 togglePosition = cardRankChoicesSecondary.GetTogglePosition(selectedRank);
                Vector3 textPosition = byTwoText.transform.position;
                byTwoText.transform.position = new Vector3(textPosition.x, togglePosition.y, textPosition.z);
            }
        }
        else
        {
            byThreeText.gameObject.SetActive(false);
            byTwoText.gameObject.SetActive(false);
        }

        if (isPrimary)
        {
            selectedPrimaryRank = selectedRank;
            if (selectedRank == selectedSecondaryRank || (isTwoPairSelection && selectedRank < selectedSecondaryRank))
            {
#if UNITY_EDITOR
                Debug.Log("must reset secondary rank selection");
#endif
                cardRankChoicesSecondary.ResetSelection();
                // cardRankChoicesPrimary.EnableRankToggles();
                // selectedSecondaryRank = null;
                byTwoText.gameObject.SetActive(false);
            }
            if (isTwoPairSelection)
            {
                cardRankChoicesSecondary.DarkenDownTo(selectedRank);
            }
        }
        else
        {
            selectedSecondaryRank = selectedRank;
            if (selectedRank == selectedPrimaryRank || (isTwoPairSelection && selectedRank > selectedPrimaryRank))
            {
#if UNITY_EDITOR
                Debug.Log("must reset primary rank selection");
#endif
                cardRankChoicesPrimary.ResetSelection();
                // cardRankChoicesSecondary.EnableRankToggles();
                // selectedPrimaryRank = null;
                byThreeText.gameObject.SetActive(false);
            }
            if (isTwoPairSelection)
            {
                cardRankChoicesPrimary.DarkenUpTo(selectedRank);
            }
        }
    }

    private void PrimaryRankChoice_NoSelectionMade()
    {
        selectedPrimaryRank = null;
    }

    private void SecondaryRankChoice_NoSelectionMade()
    {
        selectedSecondaryRank = null;
    }

    private void SuitChoice_SelectSuit(Suit selectedSuit)
    {
        this.selectedSuit = selectedSuit;
    }

    private void SuitChoice_NoSelectionMade()
    {
        selectedSuit = null;
    }

    private void HideAllSelections()
    {
        // selectedPrimaryRank = null;
        cardRankChoicesPrimary.Hide();
        // selectedSecondaryRank = null;
        cardRankChoicesSecondary.Hide();
        // selectedSuit = null;
        cardSuitChoices.Hide();

        byThreeText.gameObject.SetActive(false);
        byTwoText.gameObject.SetActive(false);
    }

    public PokerHand GetSelection()
    {
        bool? selectionsMade = selectedHand?.IsSelectionCorrect(selectedPrimaryRank, selectedSecondaryRank, selectedSuit);
        if (selectionsMade.HasValue && selectionsMade.Value)
        {
#if UNITY_EDITOR
            Debug.Log($"/ Hand={selectedHand} / PRank={selectedPrimaryRank} / SRank={selectedSecondaryRank} / Suit={selectedSuit} / ... This hand is allowed to be played");
#endif
            return PokerHandFactory.CreatePokerHand((HandType)selectedHand, selectedPrimaryRank, selectedSecondaryRank, selectedSuit);
        }
#if UNITY_EDITOR
        Debug.Log($"/ Hand={selectedHand} / PRank={selectedPrimaryRank} / SRank={selectedSecondaryRank} / Suit={selectedSuit} / ... Not a valid hand");
#endif
        return null;
    }
}
