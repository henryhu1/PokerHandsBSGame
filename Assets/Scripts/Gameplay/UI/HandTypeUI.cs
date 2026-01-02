using UnityEngine;
using UnityEngine.UI;

public class HandTypeUI : ToggleSelectionableUIBase<HandType>
{
    [Header("Firing Events")]
    [SerializeField] private HandTypeEventChannelSO OnSelectHandType;

    [Header("Listening Events")]
    [SerializeField] private BoolEventChannelSO OnAreFlushesAllowed;
    [SerializeField] private PokerHandEventChannelSO OnUpdatePlayableHands;
    [SerializeField] private PokerHandEventChannelSO OnSendPokerHandToPlay;

    private void Start()
    {
        foreach (var toggleEntry in toggleMap) {
            Toggle toggle = toggleEntry.toggle;
            toggle.onValueChanged.AddListener(isOn =>
            {
                SetToggleColor(toggle);
                if (isOn)
                {
                    OnSelectHandType.RaiseEvent(toggleEntry.type);
                }
            });
        }
    }

    private void OnEnable()
    {
        EnableAllTogglesInteractability();

        OnAreFlushesAllowed.OnEventRaised += CardManager_AreFlushesAllowed;
        OnUpdatePlayableHands.OnEventRaised += UpdatedPlayableHands;
        OnSendPokerHandToPlay.OnEventRaised += SentPokerHandToPlay;
    }

    private void OnDisable()
    {
        OnAreFlushesAllowed.OnEventRaised -= CardManager_AreFlushesAllowed;
        OnUpdatePlayableHands.OnEventRaised -= UpdatedPlayableHands;
        OnSendPokerHandToPlay.OnEventRaised -= SentPokerHandToPlay;
    }

    private void UpdatedPlayableHands(PokerHand playedHand)
    {
        HandType lastPlayed = playedHand.GetHandType();
        Rank primaryRank = playedHand.GetPrimaryRank();
        Rank secondaryRank = playedHand.GetSecondaryRank();
        if (lastPlayed == HandType.RoyalFlush)
        {
            DisableAllTogglesInteractability();
        }
        else if (primaryRank == Rank.Ace && (playedHand is SingleRankHand || playedHand is RankSuitHand || secondaryRank == Rank.King))
        {
            EnableTogglesToAtLeast(lastPlayed + 1);
        }
        else
        {
            EnableTogglesToAtLeast(lastPlayed);
        }
    }

    private void SentPokerHandToPlay(PokerHand playedPokerHand)
    {
        ResetSelection();
    }

    private void CardManager_AreFlushesAllowed(bool flushesAllowed)
    {
        Toggle flushToggle = FindToggle(HandType.Flush);

        if (flushToggle == null) return;

        ChangeToggleInteractability(flushToggle, flushesAllowed);
    }
}
