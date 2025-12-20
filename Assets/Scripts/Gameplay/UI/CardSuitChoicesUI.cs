using UnityEngine;
using UnityEngine.UI;

public class CardSuitChoicesUI : ToggleSelectionableUIBase<Suit>
{
    [Header("UI")]
    [SerializeField] private GameObject[] royalFlushImages;

    [HideInInspector]
    public delegate void SelectRankDelegateHandler(Suit selectedSuit);
    [HideInInspector]
    public event SelectRankDelegateHandler OnSelectSuit;

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
                    OnSelectSuit?.Invoke(toggleDictionary[toggle]);
                }
            });
        }
    }

    public void Show(HandType handNeedingSuit)
    {
        bool isForRoyalFlush = handNeedingSuit == HandType.RoyalFlush;
        foreach (GameObject royalFlushImage in royalFlushImages)
        {
            royalFlushImage.SetActive(isForRoyalFlush);
        }
        Show();
    }
}
