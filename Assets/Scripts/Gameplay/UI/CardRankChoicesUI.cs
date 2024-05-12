using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardRankChoicesUI : ToggleSelectionableUIBase<Rank>
{
    public static Color s_DarkerColor = new Color(0.8f, 0.8f, 0.8f);

    public static Dictionary<Hand, Rank> s_HandRankLowerBounds = new Dictionary<Hand, Rank>
    {
        { Hand.Straight, Straight.s_LowestStraight },
        { Hand.Flush, Flush.s_LowestFlush },
        { Hand.StraightFlush, StraightFlush.s_LowestStraightFlush },
    };

    // [SerializeField] private ToggleGroup m_CardRankChoiceToggleGroup;
    [SerializeField] private Toggle m_TwoToggle;
    [SerializeField] private Toggle m_ThreeToggle;
    public Toggle ThreeToggle { get { return m_ThreeToggle; } }
    [SerializeField] private Toggle m_FourToggle;
    [SerializeField] private Toggle m_FiveToggle;
    [SerializeField] private Toggle m_SixToggle;
    [SerializeField] private Toggle m_SevenToggle;
    [SerializeField] private Toggle m_EightToggle;
    [SerializeField] private Toggle m_NineTiggle;
    [SerializeField] private Toggle m_TenToggle;
    [SerializeField] private Toggle m_JackToggle;
    [SerializeField] private Toggle m_QueenToggle;
    [SerializeField] private Toggle m_KingToggle;
    public Toggle KingToggle { get { return m_KingToggle; } }
    [SerializeField] private Toggle m_AceToggle;

    [SerializeField] private bool m_isPrimaryRankChoice = false;
    // TODO: limit what hands you choose rank for
    private Hand m_choosingRankFor;
    public Hand ChoosingRankFor
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

    private void Awake()
    {
        m_byThreeText.gameObject.SetActive(false);
        m_byTwoText.gameObject.SetActive(false);

        m_ToggleDictionary = new Dictionary<Toggle, Rank>
        {
            { m_TwoToggle, Rank.Two },
            { m_ThreeToggle, Rank.Three },
            { m_FourToggle, Rank.Four },
            { m_FiveToggle, Rank.Five },
            { m_SixToggle, Rank.Six },
            { m_SevenToggle, Rank.Seven },
            { m_EightToggle, Rank.Eight },
            { m_NineTiggle, Rank.Nine },
            { m_TenToggle, Rank.Ten },
            { m_JackToggle, Rank.Jack },
            { m_QueenToggle, Rank.Queen },
            { m_KingToggle, Rank.King },
            { m_AceToggle, Rank.Ace },
        };

        m_Toggles = new List<Toggle>
        {
            m_TwoToggle,
            m_ThreeToggle,
            m_FourToggle,
            m_FiveToggle,
            m_SixToggle,
            m_SevenToggle,
            m_EightToggle,
            m_NineTiggle,
            m_TenToggle,
            m_JackToggle,
            m_QueenToggle,
            m_KingToggle,
            m_AceToggle
        };
        foreach (Toggle toggle in m_Toggles) {
            toggle.onValueChanged.AddListener((bool isOn) =>
            {
                SetToggleColor(toggle);
                if (isOn)
                {
                    // Rank selected, passing if this is the primary rank
                    OnSelectRank?.Invoke(m_isPrimaryRankChoice, m_ToggleDictionary[toggle]);
                    if (ChoosingRankFor == Hand.FullHouse)
                    {
                        if (m_isPrimaryRankChoice)
                        {
                            m_byThreeText.gameObject.SetActive(true);
                            Vector3 textPosition = m_byThreeText.gameObject.transform.position;
                            m_byThreeText.gameObject.transform.position = new Vector3(textPosition.x, toggle.gameObject.transform.position.y, textPosition.z);
                        }
                        else
                        {
                            m_byTwoText.gameObject.SetActive(true);
                            Vector3 textPosition = m_byTwoText.gameObject.transform.position;
                            m_byTwoText.gameObject.transform.position = new Vector3(textPosition.x, toggle.gameObject.transform.position.y, textPosition.z);
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
                    if (m_ToggleGroup.GetFirstActiveToggle() == null)
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
        if (s_HandRankLowerBounds.TryGetValue(ChoosingRankFor, out Rank rankBound)) {
            EnableTogglesToAtLeast(RankToToggleIndex(rankBound));
        }
        //else if (ChoosingRankFor == Hand.TwoPair)
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
            EnableTogglesToAtLeast();
        }
    }

    public void DarkenDownTo(Rank highestRank)
    {
        foreach (Toggle toggle in m_Toggles)
        {
            if (!toggle.enabled) continue;

            if (m_ToggleDictionary[toggle] < highestRank)
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
        foreach (Toggle toggle in m_Toggles)
        {
            if (!toggle.enabled) continue;

            if (m_ToggleDictionary[toggle] > lowestRank)
            {
                toggle.image.color = Color.white;
            }
            else
            {
                toggle.image.color = s_DarkerColor;
            }
        }
    }

    private static int RankToToggleIndex(Rank rank)
    {
        return (int)rank - 2;
    }

}
