using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SelectionUI<T> : MonoBehaviour
{
    public static SelectionUI<T> Instance;

    private ToggleColors m_ToggleColors = new ToggleColors();

    [SerializeField] protected ToggleGroup m_ToggleGroup;

    protected List<Toggle> m_Toggles;
    protected Dictionary<Toggle, T> m_ToggleDictionary;
    // protected Dictionary<Toggle, int> m_ToggleLevels;

    [HideInInspector]
    public delegate void NoSelectionMadeDelegateHandler();
    [HideInInspector]
    public event NoSelectionMadeDelegateHandler OnNoSelectionMade;


    // TODO: have virtual methods for parent class?
    public virtual void Somthing()
    {
        Debug.Log("example of a virtual method");
    }

    private void Awake()
    {
        Instance = this;

    }

    protected void SetToggleColor(Toggle toggle)
    {
        ColorBlock toggleColorBlock = toggle.colors;
        toggleColorBlock.normalColor = m_ToggleColors.ToggleColorNormal;
        toggleColorBlock.highlightedColor = m_ToggleColors.ToggleColorHighlighted;
        toggleColorBlock.pressedColor = m_ToggleColors.ToggleColorPressed;
        toggleColorBlock.selectedColor = m_ToggleColors.ToggleColorSelected;
        m_ToggleColors.ChangeToggleColors(toggle.colors);
        toggle.colors = toggleColorBlock;
    }

    //public bool IsSelectionMade()
    //{
    //    IEnumerable<Toggle> selectedToggle = m_ToggleGroup.ActiveToggles();
    //    return isActiveAndEnabled && selectedToggle != null && selectedToggle.Any();
    //}

    //public T GetSelection()
    //{
    //    IEnumerable<Toggle> selectedToggle = m_ToggleGroup.ActiveToggles();

    //    return m_ToggleDictionary[selectedToggle.First()];
    //}

    public void InvokeNoSelectionMade()
    {
        OnNoSelectionMade?.Invoke();
    }

    public void ResetSelection()
    {
        m_ToggleGroup.SetAllTogglesOff();
    }

    protected void EnableTogglesToAtLeast(int level = 0)
    {
        EnableTogglesTo(level);
    }

    protected void EnableTogglesToAtMost(int level = int.MaxValue)
    {
        EnableTogglesTo(level, false);
    }

    private void EnableTogglesTo(int level = 0, bool higherLevel = true)
    {
        for (int i = 0; i < m_Toggles.Count; i++)
        {
            bool shouldEnable = higherLevel ? i >= level : i <= level;
            m_Toggles[i].enabled = shouldEnable;
            m_Toggles[i].image.color = shouldEnable ? Color.white : ToggleColors.k_DisabledColor;
        }
    }

    public void Show()
    {
        gameObject.SetActive(true);
        ResetSelection();
    }

    public void Hide()
    {
        ResetSelection();
        gameObject.SetActive(false);
    }
}
