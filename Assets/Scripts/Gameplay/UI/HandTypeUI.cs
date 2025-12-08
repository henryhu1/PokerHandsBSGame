using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HandTypeUI : ToggleSelectionableUIBase<Hand>
{
    public static HandTypeUI Instance { get; private set; }

    // [SerializeField] private ToggleGroup m_HandTypeToggleGroup;
    [SerializeField] private Toggle m_HighCardToggle;
    [SerializeField] private Toggle m_PairToggle;
    [SerializeField] private Toggle m_TwoPairToggle;
    [SerializeField] private Toggle m_ThreeOfAKindToggle;
    [SerializeField] private Toggle m_StraightToggle;
    [SerializeField] private Toggle m_FlushToggle;
    [SerializeField] private Toggle m_FullHouseToggle;
    [SerializeField] private Toggle m_FourOfAKindToggle;
    [SerializeField] private Toggle m_StraightFlushToggle;
    [SerializeField] private Toggle m_RoyalFlushToggle;

    private Dictionary<Toggle, Action> m_ToggleInvokes;

    [HideInInspector]
    public delegate void NeedCardRankChoicesPrimaryDelegateHandler(Hand selectedHand); //, int rankLevel = 0);
    [HideInInspector]
    public event NeedCardRankChoicesPrimaryDelegateHandler OnNeedCardRankChoicesPrimary;

    [HideInInspector]
    public delegate void NeedCardRankChoicesSecondaryDelegateHandler(Hand selectedHand, bool isFullHouse); //, int rankPrimaryLevel = 0, int rankSecondaryLevel = 0);
    [HideInInspector]
    public event NeedCardRankChoicesSecondaryDelegateHandler OnNeedCardRankChoicesSecondary;

    [HideInInspector]
    public delegate void NeedCardSuitChoicesDelegateHandler(Hand selectedHand); //, int rankLevel = 0);
    [HideInInspector]
    public event NeedCardSuitChoicesDelegateHandler OnNeedCardSuitChoices;

    [HideInInspector]
    public delegate void NeedRoyalFlushChoicesDelegateHandler();
    [HideInInspector]
    public event NeedRoyalFlushChoicesDelegateHandler OnNeedRoyalFlushChoices;

    [Header("Listening Events")]
    [SerializeField] private BoolEventChannelSO OnAreFlushesAllowed;

    private void Awake()
    {
        if (Instance != this && Instance != null)
        {
            Destroy(Instance.gameObject);
        }
        Instance = this;

        m_ToggleDictionary = new Dictionary<Toggle, Hand>
        {
            { m_HighCardToggle, Hand.HighCard },
            { m_PairToggle, Hand.Pair },
            { m_TwoPairToggle, Hand.TwoPair },
            { m_ThreeOfAKindToggle, Hand.ThreeOfAKind },
            { m_StraightToggle, Hand.Straight },
            { m_FlushToggle, Hand.Flush },
            { m_FullHouseToggle, Hand.FullHouse },
            { m_FourOfAKindToggle, Hand.FourOfAKind },
            { m_StraightFlushToggle, Hand.StraightFlush },
            { m_RoyalFlushToggle, Hand.RoyalFlush },
        };

        m_ToggleInvokes = new Dictionary<Toggle, Action>
        {
            { m_HighCardToggle, () => OnNeedCardRankChoicesPrimary?.Invoke(Hand.HighCard) },
            { m_PairToggle, () => OnNeedCardRankChoicesPrimary?.Invoke(Hand.Pair) },
            { m_TwoPairToggle, () => OnNeedCardRankChoicesSecondary?.Invoke(Hand.TwoPair, false) },
            { m_ThreeOfAKindToggle, () => OnNeedCardRankChoicesPrimary?.Invoke(Hand.ThreeOfAKind) },
            { m_StraightToggle, () => OnNeedCardRankChoicesPrimary?.Invoke(Hand.Straight) },
            { m_FlushToggle, () => OnNeedCardSuitChoices?.Invoke(Hand.Flush) },
            { m_FullHouseToggle, () => OnNeedCardRankChoicesSecondary?.Invoke(Hand.FullHouse, true) },
            { m_FourOfAKindToggle, () => OnNeedCardRankChoicesPrimary?.Invoke(Hand.FourOfAKind) },
            { m_StraightFlushToggle, () => OnNeedCardSuitChoices?.Invoke(Hand.StraightFlush) },
            { m_RoyalFlushToggle, () => OnNeedRoyalFlushChoices?.Invoke() },
        };

        m_Toggles = new List<Toggle>
        {
            m_HighCardToggle,
            m_PairToggle,
            m_TwoPairToggle,
            m_ThreeOfAKindToggle,
            m_StraightToggle,
            m_FlushToggle,
            m_FullHouseToggle,
            m_FourOfAKindToggle,
            m_StraightFlushToggle,
            m_RoyalFlushToggle
        };
        foreach (Toggle toggle in m_Toggles) {
            toggle.onValueChanged.AddListener((bool isOn) =>
            {
                SetToggleColor(toggle);
                if (isOn)               // Toggle is selected to be on
                {
                    m_ToggleInvokes[toggle]();
                }
                else                    // Toggle is selected to be off
                {
                    if (m_ToggleGroup.GetFirstActiveToggle() == null)
                    {
                        InvokeNoSelectionMade();
                    }
                }
            });
        }
    }

    private void Start()
    {
        HandSelectionUI.Instance.RegisterHandTypeUICallbacks();
    }

    private void OnDestroy()
    {
        HandSelectionUI.Instance.UnregisterHandTypeUICallbacks();
    }

    private void OnEnable()
    {
        OnAreFlushesAllowed.OnEventRaised += CardManager_AreFlushesAllowed;
    }

    private void OnDisable()
    {
        OnAreFlushesAllowed.OnEventRaised -= CardManager_AreFlushesAllowed;
    }

    // TODO: disabling all hands lower than last played hand
    //private void GameManager_UpdatePlayableHands(PokerHand playedHand)
    //{
    //    EnableTogglesToAtLeast();
    //}

    private void CardManager_AreFlushesAllowed(bool flushesAllowed)
    {
        m_FlushToggle.enabled = flushesAllowed;
        m_FlushToggle.image.color = flushesAllowed ? Color.white : ToggleColors.k_DisabledColor;
    }

    //private static int RankToToggleIndex(Rank rank)
    //{
    //    return (int)rank - 2;
    //}
}
