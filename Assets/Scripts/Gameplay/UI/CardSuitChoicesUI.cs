using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class CardSuitChoicesUI : SelectionUI<Suit>
{
    // [SerializeField] private ToggleGroup m_CardSuitChoiceToggleGroup;
    [SerializeField] private Toggle m_ClubsToggle;
    [SerializeField] private Toggle m_DiamondsToggle;
    [SerializeField] private Toggle m_HeartsToggle;
    [SerializeField] private Toggle m_SpadesToggle;

    [SerializeField] private Hand m_choosingSuitFor;
    // TODO: limit what hands you choose suit for, ie. Flush, Straight Flush, Royal Flush
    public Hand ChoosingSuitFor
    {
        get { return m_choosingSuitFor; }
        set { m_choosingSuitFor = value; }
    }

    [HideInInspector]
    public delegate void SelectRankDelegateHandler(Suit selectedSuit);
    [HideInInspector]
    public event SelectRankDelegateHandler OnSelectSuit;

    // private Dictionary<Toggle, Suit> m_ToggleSelectedSuit;

    private void Awake()
    {
        Instance = this;

        m_ToggleDictionary = new Dictionary<Toggle, Suit>
        {
            { m_ClubsToggle, Suit.Club },
            { m_DiamondsToggle, Suit.Diamond },
            { m_HeartsToggle, Suit.Heart },
            { m_SpadesToggle, Suit.Spade },
        };

        m_Toggles = new List<Toggle> { m_ClubsToggle, m_DiamondsToggle, m_HeartsToggle, m_SpadesToggle };
        foreach (Toggle toggle in m_Toggles) {
            toggle.onValueChanged.AddListener((bool isOn) =>
            {
                SetToggleColor(toggle);
                if (isOn)
                {
                    OnSelectSuit?.Invoke(m_ToggleDictionary[toggle]);
                }
                else
                {
                    if (m_ToggleGroup.GetFirstActiveToggle() == null)
                    {
                        InvokeNoSelectionMade();
                    }
                }
            });
        }
    }
}
