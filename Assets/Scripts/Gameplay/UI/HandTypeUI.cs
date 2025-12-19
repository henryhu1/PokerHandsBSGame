using UnityEngine;
using UnityEngine.UI;

public class HandTypeUI : ToggleSelectionableUIBase<HandType>
{
    [Header("Firing Events")]
    [SerializeField] private HandTypeEventChannelSO OnSelectHandType;

    [Header("Listening Events")]
    [SerializeField] private BoolEventChannelSO OnAreFlushesAllowed;

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
                    OnSelectHandType.RaiseEvent(toggleDictionary[toggle]);
                }
                else
                {
                    if (toggleGroup.GetFirstActiveToggle() == null)
                    {
                        InvokeNoSelectionMade();
                    }
                }
            });
        }
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
        Toggle flushToggle = FindToggle(HandType.Flush);

        if (flushToggle == null) return;

        flushToggle.interactable = flushesAllowed;
        flushToggle.image.color = flushesAllowed ? Color.white : ToggleColors.k_DisabledColor;
    }
}
