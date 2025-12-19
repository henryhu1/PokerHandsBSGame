using System.Collections.Generic;
using TMPro;
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
    // TODO: limit what hands you choose rank for
    private HandType m_choosingRankFor;
    public HandType ChoosingRankFor
    {
        get { return m_choosingRankFor; }
        set { m_choosingRankFor = value; }
    }

    [SerializeField] private TextMeshProUGUI m_byThreeText;
    [SerializeField] private TextMeshProUGUI m_byTwoText;

    [HideInInspector]
    public delegate void SelectRankDelegateHandler(bool isPrimary, Rank selectedRank);
    [HideInInspector]
    public event SelectRankDelegateHandler OnSelectRank;

    protected override void Awake()
    {
        base.Awake();

        m_byThreeText.gameObject.SetActive(false);
        m_byTwoText.gameObject.SetActive(false);

        foreach (var toggleEntry in toggleDictionary) {
            Toggle toggle = toggleEntry.Key;
            toggle.onValueChanged.AddListener(isOn =>
            {
                SetToggleColor(toggle);
                if (isOn)
                {
                    // Rank selected, passing if this is the primary rank
                    OnSelectRank?.Invoke(m_isPrimaryRankChoice, toggleDictionary[toggle]);
                    if (ChoosingRankFor == HandType.FullHouse)
                    {
                        if (m_isPrimaryRankChoice)
                        {
                            m_byThreeText.gameObject.SetActive(true);
                            Vector3 textPosition = m_byThreeText.transform.position;
                            m_byThreeText.transform.position = new Vector3(textPosition.x, toggle.transform.position.y, textPosition.z);
                        }
                        else
                        {
                            m_byTwoText.gameObject.SetActive(true);
                            Vector3 textPosition = m_byTwoText.transform.position;
                            m_byTwoText.transform.position = new Vector3(textPosition.x, toggle.transform.position.y, textPosition.z);
                        }
                    }
                    else
                    {
                        m_byThreeText.gameObject.SetActive(false);
                        m_byTwoText.gameObject.SetActive(false);
                    }
                }
                else
                {
                    if (toggleGroup.GetFirstActiveToggle() == null)
                    {
                        InvokeNoSelectionMade();
                        m_byThreeText.gameObject.SetActive(false);
                        m_byTwoText.gameObject.SetActive(false);
                    }
                }
            });
        }
    }

    public void EnableRankToggles()
    {
        if (s_HandTypeRankLowerBounds.TryGetValue(ChoosingRankFor, out Rank rankBound)) {
            EnableTogglesToAtLeast(rankBound);
        }
        //else if (ChoosingRankFor == HandType.TwoPair)
        //{
        //    if (m_isPrimaryRankChoice)
        //    {
        //        EnableTogglesToAtLeast(RankToToggleIndex(m_ToggleDictionary[ThreeToggle]));
        //    }
        //    else
        //    {
        //        EnableTogglesToAtMost(RankToToggleIndex(m_ToggleDictionary[KingToggle]));
        //    }
        //}
        else
        {
            EnableTogglesToAtLeast(Rank.Two);
        }
    }

    public void DarkenDownTo(Rank highestRank)
    {
        foreach (var toggleEntry in toggleDictionary)
        {
            Toggle toggle = toggleEntry.Key;
            if (!toggle.enabled) continue;

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
            if (!toggle.enabled) continue;

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
