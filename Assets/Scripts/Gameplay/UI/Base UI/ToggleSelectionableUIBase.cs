using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class ToggleSelectionableUIBase<T> : MonoBehaviour where T : Enum
{
    private ToggleColors toggleColors = new();

    [SerializeField] protected ToggleGroup toggleGroup;

    [Serializable]
    protected class ToggleMapEntry
    {
        public Toggle toggle;
        public T type;
    }

    [SerializeField] private ToggleMapEntry[] toggleMap;

    protected Dictionary<Toggle, T> toggleDictionary = new();

    [Header("Firing Events")]
    [SerializeField] private VoidEventChannelSO OnNoSelectionMade;

    protected virtual void Awake()
    {
        foreach (ToggleMapEntry toggleEntry in toggleMap)
        {
            toggleDictionary.Add(toggleEntry.toggle, toggleEntry.type);
        }
    }

    protected void SetToggleColor(Toggle toggle)
    {
        ColorBlock toggleColorBlock = toggle.colors;
        toggleColorBlock.normalColor = toggleColors.toggleColorNormal;
        toggleColorBlock.highlightedColor = toggleColors.toggleColorHighlighted;
        toggleColorBlock.pressedColor = toggleColors.toggleColorPressed;
        toggleColorBlock.selectedColor = toggleColors.toggleColorSelected;
        toggleColors.ChangeToggleColors(toggle.colors);
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
        OnNoSelectionMade.RaiseEvent();
    }

    public void ResetSelection()
    {
        toggleGroup.SetAllTogglesOff();
    }

    protected Toggle FindToggle(T value)
    {
        foreach (var toggleEntry in toggleDictionary)
        {
            if (toggleEntry.Value.Equals(value))
            {
                return toggleEntry.Key;
            }
        }
        return null;
    }

    protected void EnableTogglesToAtLeast(T level)
    {
        foreach (var toggleEntry in toggleDictionary)
        {
            Toggle toggle = toggleEntry.Key;
            T type = toggleEntry.Value;
            bool shouldEnable = type.CompareTo(level) >= 0;
            toggle.enabled = shouldEnable;
            toggle.image.color = shouldEnable ? Color.white : ToggleColors.k_DisabledColor;
        }
    }

    protected void EnableTogglesToAtMost(T level)
    {
        foreach (var toggleEntry in toggleDictionary)
        {
            Toggle toggle = toggleEntry.Key;
            T type = toggleEntry.Value;
            bool shouldEnable = type.CompareTo(level) <= 0;
            toggle.enabled = shouldEnable;
            toggle.image.color = shouldEnable ? Color.white : ToggleColors.k_DisabledColor;
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
