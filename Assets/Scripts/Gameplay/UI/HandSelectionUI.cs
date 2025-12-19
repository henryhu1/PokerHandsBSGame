using CardTraitExtensions;
using UnityEngine;

public class HandSelectionUI : MonoBehaviour
{
    // TODO: remove Singleton?
    public static HandSelectionUI Instance { get; private set; }

    [Header("UI")]
    [SerializeField] private CardRankChoicesUI cardRankChoicesPrimary;
    [SerializeField] private CardRankChoicesUI cardRankChoicesSecondary;
    [SerializeField] private CardSuitChoicesUI cardSuitChoices;

    [Header("Listening Events")]
    [SerializeField] private HandTypeEventChannelSO OnSelectHandType;
    [SerializeField] private VoidEventChannelSO OnNoSelectionHand;
    [SerializeField] private VoidEventChannelSO OnNoSelectionPrimaryRank;
    [SerializeField] private VoidEventChannelSO OnNoSelectionSecondaryRank;
    [SerializeField] private VoidEventChannelSO OnNoSelectionSuit;

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

        ResetAllSelections();
    }

    private void OnEnable()
    {
        cardRankChoicesPrimary.OnSelectRank += RankChoice_SelectRank;
        OnNoSelectionPrimaryRank.OnEventRaised += PrimaryRankChoice_NoSelectionMade;
        cardRankChoicesSecondary.OnSelectRank += RankChoice_SelectRank;
        OnNoSelectionSecondaryRank.OnEventRaised += SecondaryRankChoice_NoSelectionMade;
        cardSuitChoices.OnSelectSuit += SuitChoice_SelectSuit;
        OnNoSelectionSuit.OnEventRaised += SuitChoice_NoSelectionMade;

        OnSelectHandType.OnEventRaised += HandleHandTypeSelection;
        OnNoSelectionHand.OnEventRaised += HandTypeUI_NoSelectionMade;
    }

    private void OnDisable()
    {
        cardRankChoicesPrimary.OnSelectRank -= RankChoice_SelectRank;
        OnNoSelectionPrimaryRank.OnEventRaised -= PrimaryRankChoice_NoSelectionMade;
        cardRankChoicesSecondary.OnSelectRank -= RankChoice_SelectRank;
        OnNoSelectionSecondaryRank.OnEventRaised -= SecondaryRankChoice_NoSelectionMade;
        cardSuitChoices.OnSelectSuit -= SuitChoice_SelectSuit;
        OnNoSelectionSuit.OnEventRaised -= SuitChoice_NoSelectionMade;

        OnSelectHandType.OnEventRaised -= HandleHandTypeSelection;
        OnNoSelectionHand.OnEventRaised -= HandTypeUI_NoSelectionMade;
    }

    private void HandleHandTypeSelection(HandType newHandSelected)
    {
        ResetAllSelections();
        selectedHand = newHandSelected;

        switch (selectedHand)
        {
            case HandType.RoyalFlush:
                cardSuitChoices.Show(HandType.RoyalFlush);
                break;
            case HandType.TwoPair:
            case HandType.FullHouse:
                cardRankChoicesSecondary.Show();
                // TODO: ensure lower level hands are not allowed to be selected
                cardRankChoicesSecondary.ChoosingRankFor = newHandSelected;
                cardRankChoicesSecondary.EnableRankToggles();

                cardRankChoicesPrimary.Show();
                cardRankChoicesPrimary.ChoosingRankFor = newHandSelected;
                cardRankChoicesPrimary.EnableRankToggles();
                break;
            case HandType.Flush:
            case HandType.StraightFlush:
                cardSuitChoices.Show(newHandSelected);

                cardRankChoicesPrimary.Show();
                cardRankChoicesPrimary.ChoosingRankFor = newHandSelected;
                cardRankChoicesPrimary.EnableRankToggles();
                break;
            default:
                cardRankChoicesPrimary.Show();
                cardRankChoicesPrimary.ChoosingRankFor = newHandSelected;
                cardRankChoicesPrimary.EnableRankToggles();
                break;
        }
    }

    private void HandTypeUI_NoSelectionMade()
    {
        ResetAllSelections();
    }

    private void RankChoice_SelectRank(bool isPrimary, Rank selectedRank)
    {
#if UNITY_EDITOR
        Debug.Log($"rank selected, primary={isPrimary} rank={selectedRank}");
#endif
        // bool isTwoPairSelection = selectedHand != null && selectedHand.Equals(Hand.TwoPair);
        if (isPrimary)
        {
            selectedPrimaryRank = selectedRank;
            if (selectedRank == selectedSecondaryRank) // || (isTwoPairSelection && selectedRank < selectedSecondaryRank))
            {
#if UNITY_EDITOR
                Debug.Log("must reset secondary rank selection");
#endif
                cardRankChoicesSecondary.ResetSelection();
                // CardRankChoicesPrimary.EnableRankToggles();
                selectedSecondaryRank = null;
            }
            // if (isTwoPairSelection)
            // {
            //     CardRankChoicesSecondary.DarkenDownTo(selectedRank);
            // }
        }
        else
        {
            selectedSecondaryRank = selectedRank;
            if (selectedRank == selectedPrimaryRank) // || (isTwoPairSelection && selectedRank > selectedPrimaryRank))
            {
#if UNITY_EDITOR
                Debug.Log("must reset primary rank selection");
#endif
                cardRankChoicesPrimary.ResetSelection();
                // CardRankChoicesSecondary.EnableRankToggles();
                selectedPrimaryRank = null;
            }
            // if (isTwoPairSelection)
            // {
            //     CardRankChoicesPrimary.DarkenUpTo(selectedRank);
            // }
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

    private void ResetAllSelections()
    {
        selectedPrimaryRank = null;
        cardRankChoicesPrimary.Hide();
        selectedSecondaryRank = null;
        cardRankChoicesSecondary.Hide();
        selectedSuit = null;
        cardSuitChoices.Hide();
    }

    public PokerHand GetSelection()
    {
        bool? selectionsMade = selectedHand?.IsSelectionCorrect(selectedPrimaryRank, selectedSecondaryRank, selectedSuit);
        if (selectionsMade.HasValue && selectionsMade.Value)
        {
#if UNITY_EDITOR
            Debug.Log($"Hand={selectedHand} PRank={selectedPrimaryRank} SRank={selectedSecondaryRank} Suit={selectedSuit}: This hand is allowed to be played");
#endif
            return PokerHandFactory.CreatePokerHand((HandType)selectedHand, selectedPrimaryRank, selectedSecondaryRank, selectedSuit);
        }
#if UNITY_EDITOR
        Debug.Log($"Hand={selectedHand} PRank={selectedPrimaryRank} SRank={selectedSecondaryRank} Suit={selectedSuit}: Not a valid hand");
#endif
        return null;
    }
}
