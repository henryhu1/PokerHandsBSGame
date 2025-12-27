using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardRankChoicesUI : ToggleSelectionableUIBase<Rank>
{
    public static Color s_DarkerColor = new(0.8f, 0.8f, 0.8f);

    public static Dictionary<HandType, Rank> s_HandTypeRankLowerBounds = new()
    {
        { HandType.Straight, Straight.s_LowestStraight },
        { HandType.Flush, Flush.s_LowestFlush },
        { HandType.StraightFlush, StraightFlush.s_LowestStraightFlush },
    };

    [SerializeField] private bool m_isPrimaryRankChoice = false;

    [Header("Firing Events")]
    [SerializeField] private RankEventChannelSO OnSelectRank;

    protected override void Awake()
    {
        base.Awake();

        foreach (var toggleEntry in toggleDictionary) {
            Toggle toggle = toggleEntry.Key;
            toggle.onValueChanged.AddListener(isOn =>
            {
                SetToggleColor(toggle);
                if (isOn)
                {
                    // Rank selected, passing if this is the primary rank
                    OnSelectRank.RaiseEvent(toggleDictionary[toggle]);
                }
            });
        }
    }

    private void DisableUnplayableRanks(PokerHand lowerBoundHand)
    {
        Rank lowerBoundRank;
        Rank primaryRank = lowerBoundHand.GetPrimaryRank();
        Rank secondaryRank = lowerBoundHand.GetSecondaryRank();
        if (m_isPrimaryRankChoice)
        {
            if (lowerBoundHand is DoubleRankHand)
            {
                if (primaryRank == Rank.Two && secondaryRank == Rank.Ace)
                {
                    lowerBoundRank = primaryRank;
                }
                else
                {
                    lowerBoundRank = primaryRank - 1;
                }
            }
            else
            {
                lowerBoundRank = primaryRank;
            }
        }
        else
        {
            lowerBoundRank = secondaryRank;
        }

        if (lowerBoundRank != Rank.Ace)
        {
            EnableTogglesToAtLeast(lowerBoundRank + 1);
        }
        else
        {
            DisableAllTogglesInteractability();
        }
    }

    public void Show(HandType choosingRankForHandType)
    {
        PokerHand lastPlayedHand = GameManager.Instance.GetLastPlayedHand();
        if (lastPlayedHand != null && choosingRankForHandType == lastPlayedHand.GetHandType())
        {
            DisableUnplayableRanks(lastPlayedHand);
        }
        else if (s_HandTypeRankLowerBounds.TryGetValue(choosingRankForHandType, out Rank rankBound))
        {
            EnableTogglesToAtLeast(rankBound);
        }
        else
        {
            EnableAllTogglesInteractability();
        }

        if (choosingRankForHandType == HandType.TwoPair)
        {
            if (m_isPrimaryRankChoice)
            {
                ChangeToggleInteractability(FindToggle(Rank.Three), false);
            }
            else
            {
                ChangeToggleInteractability(FindToggle(Rank.King), false);
            }
        }
        Show();
    }

    public void DarkenDownTo(Rank highestRank)
    {
        foreach (var toggleEntry in toggleDictionary)
        {
            Toggle toggle = toggleEntry.Key;
            if (!toggle.interactable || !toggle.enabled) continue;

            if (toggleDictionary[toggle] < highestRank)
            {
                toggle.image.color = Color.white;
            }
            else
            {
                toggle.image.color = s_DarkerColor;
            }
        }
    }

    public void DarkenUpTo(Rank lowestRank)
    {
        foreach (var toggleEntry in toggleDictionary)
        {
            Toggle toggle = toggleEntry.Key;
            if (!toggle.interactable || !toggle.enabled) continue;

            if (toggleDictionary[toggle] > lowestRank)
            {
                toggle.image.color = Color.white;
            }
            else
            {
                toggle.image.color = s_DarkerColor;
            }
        }
    }
}
