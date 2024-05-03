using CardTraitExtensions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandSelectionUI : MonoBehaviour
{
    public static HandSelectionUI Instance { get; private set; }

    [SerializeField] private CardRankChoicesUI m_CardRankChoicesPrimary;
    [SerializeField] private CardRankChoicesUI m_CardRankChoicesSecondary;
    [SerializeField] private CardSuitChoicesUI m_CardSuitChoices;
    [SerializeField] private CardSuitChoicesUI m_RoyalFlushChoices;

    private Rank? m_selectedPrimaryRank;
    private Rank? m_selectedSecondaryRank;
    private Suit? m_selectedSuit;
    private Hand? m_selectedHand;

    private void Awake()
    {
        Instance = this;

        ResetAllSelections();
    }

    private void Start()
    {
        HandTypeUI.Instance.OnNeedCardRankChoicesPrimary += HandTypeUI_NeedCardRankChoicesPrimary;
        HandTypeUI.Instance.OnNeedCardRankChoicesSecondary += HandTypeUI_NeedCardRankChoicesSecondary;
        HandTypeUI.Instance.OnNeedCardSuitChoices += HandTypeUI_NeedCardSuitChoices;
        HandTypeUI.Instance.OnNeedRoyalFlushChoices += HandTypeUI_NeedRoyalFlushChoices;
        HandTypeUI.Instance.OnNoSelectionMade += HandTypeUI_NoSelectionMade;

        m_CardRankChoicesPrimary.OnSelectRank += RankChoice_SelectRank;
        m_CardRankChoicesPrimary.OnNoSelectionMade += PrimaryRankChoice_NoSelectionMade;
        m_CardRankChoicesSecondary.OnSelectRank += RankChoice_SelectRank;
        m_CardRankChoicesSecondary.OnNoSelectionMade += SecondaryRankChoice_NoSelectionMade;
        m_CardSuitChoices.OnSelectSuit += SuitChoice_SelectSuit;
        m_CardSuitChoices.OnNoSelectionMade += SuitChoice_NoSelectionMade;
        m_RoyalFlushChoices.OnSelectSuit += RoyalFlushChoice_SelectSuit;
        m_RoyalFlushChoices.OnNoSelectionMade += RoyalFlushChoice_NoSelectionMade;
    }

    private void HandTypeUI_NeedCardRankChoicesPrimary(Hand selectedHand)
    {
        ResetAllSelections();
        m_selectedHand = selectedHand;

        m_CardRankChoicesPrimary.Show();
        m_CardRankChoicesPrimary.ChoosingRankFor = selectedHand;
        m_CardRankChoicesPrimary.EnableRankToggles();
    }

    private void HandTypeUI_NeedCardRankChoicesSecondary(Hand selectedHand, bool isFullHouse)
    {
        ResetAllSelections();
        m_selectedHand = selectedHand;

        m_CardRankChoicesSecondary.Show();
        // TODO: ensure lower level hands are not allowed to be selected
        m_CardRankChoicesSecondary.ChoosingRankFor = selectedHand;
        m_CardRankChoicesSecondary.EnableRankToggles();

        m_CardRankChoicesPrimary.Show();
        m_CardRankChoicesPrimary.ChoosingRankFor = selectedHand;
        m_CardRankChoicesPrimary.EnableRankToggles();
    }

    private void HandTypeUI_NeedCardSuitChoices(Hand selectedHand)
    {
        ResetAllSelections();
        m_selectedHand = selectedHand;

        m_CardSuitChoices.Show();
        m_CardSuitChoices.ChoosingSuitFor = selectedHand;

        m_CardRankChoicesPrimary.Show();
        m_CardRankChoicesPrimary.ChoosingRankFor = selectedHand;
        m_CardRankChoicesPrimary.EnableRankToggles();
    }

    private void HandTypeUI_NeedRoyalFlushChoices()
    {
        ResetAllSelections();
        m_selectedHand = Hand.RoyalFlush;

        m_RoyalFlushChoices.Show();
    }

    private void HandTypeUI_NoSelectionMade()
    {
        ResetAllSelections();
    }

    private void RankChoice_SelectRank(bool isPrimary, Rank selectedRank)
    {
        Debug.Log($"rank selected, primary={isPrimary} rank={selectedRank}");
        // bool isTwoPairSelection = m_selectedHand != null && m_selectedHand.Equals(Hand.TwoPair);
        if (isPrimary)
        {
            m_selectedPrimaryRank = selectedRank;
            if (selectedRank == m_selectedSecondaryRank) // || (isTwoPairSelection && selectedRank < m_selectedSecondaryRank))
            {
                Debug.Log("must reset secondary rank selection");
                m_CardRankChoicesSecondary.ResetSelection();
                // m_CardRankChoicesPrimary.EnableRankToggles();
                m_selectedSecondaryRank = null;
            }
            // if (isTwoPairSelection)
            // {
            //     m_CardRankChoicesSecondary.DarkenDownTo(selectedRank);
            // }
        }
        else
        {
            m_selectedSecondaryRank = selectedRank;
            if (selectedRank == m_selectedPrimaryRank) // || (isTwoPairSelection && selectedRank > m_selectedPrimaryRank))
            {
                Debug.Log("must reset primary rank selection");
                m_CardRankChoicesPrimary.ResetSelection();
                // m_CardRankChoicesSecondary.EnableRankToggles();
                m_selectedPrimaryRank = null;
            }
            // if (isTwoPairSelection)
            // {
            //     m_CardRankChoicesPrimary.DarkenUpTo(selectedRank);
            // }
        }
    }

    private void PrimaryRankChoice_NoSelectionMade()
    {
        m_selectedPrimaryRank = null;
    }

    private void SecondaryRankChoice_NoSelectionMade()
    {
        m_selectedSecondaryRank = null;
    }

    private void SuitChoice_SelectSuit(Suit selectedSuit)
    {
        m_selectedSuit = selectedSuit;
    }

    private void SuitChoice_NoSelectionMade()
    {
        m_selectedSuit = null;
    }

    private void RoyalFlushChoice_SelectSuit(Suit selectedSuit)
    {
        // m_selectedRoyalFlush = selectedSuit;
        m_selectedSuit = selectedSuit;
    }

    private void RoyalFlushChoice_NoSelectionMade()
    {
        // m_selectedRoyalFlush = null;
        m_selectedSuit = null;
    }

    private void ResetAllSelections()
    {
        m_selectedPrimaryRank = null;
        m_CardRankChoicesPrimary.Hide();
        m_selectedSecondaryRank = null;
        m_CardRankChoicesSecondary.Hide();
        m_selectedSuit = null;
        m_CardSuitChoices.Hide();
        m_RoyalFlushChoices.Hide();
    }

    public PokerHand GetSelection()
    {
        bool? selectionsMade = m_selectedHand?.IsSelectionCorrect(m_selectedPrimaryRank, m_selectedSecondaryRank, m_selectedSuit);
        if (selectionsMade.HasValue && selectionsMade.Value)
        {
            Debug.Log($"Hand={m_selectedHand} PRank={m_selectedPrimaryRank} SRank={m_selectedSecondaryRank} Suit={m_selectedSuit}: This hand is allowed to be played");
            return PokerHandFactory.CreatePokerHand((Hand)m_selectedHand, m_selectedPrimaryRank, m_selectedSecondaryRank, m_selectedSuit);
        }
        Debug.Log($"Hand={m_selectedHand} PRank={m_selectedPrimaryRank} SRank={m_selectedSecondaryRank} Suit={m_selectedSuit}: Not a valid hand");
        return null;
    }
}
