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

    private void Start()
    {
        foreach (var toggleEntry in toggleMap) {
            Toggle toggle = toggleEntry.toggle;
            toggle.onValueChanged.AddListener(isOn =>
            {
                SetToggleColor(toggle);
                if (isOn)
                {
                    OnSelectSuit?.Invoke(toggleEntry.type);
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
