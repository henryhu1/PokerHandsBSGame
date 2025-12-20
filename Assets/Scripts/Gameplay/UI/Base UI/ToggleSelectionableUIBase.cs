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

    public void ResetSelection()
    {
        toggleGroup.SetAllTogglesOff();
        OnNoSelectionMade.RaiseEvent();
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

    public Vector3 GetTogglePosition(T value)
    {
        Toggle toggle = FindToggle(value);
        if (toggle == null) return Vector3.zero;
        return toggle.transform.position;
    }

    protected void ChangeToggleInteractability(Toggle targetToggle, bool interactable)
    {
        if (toggleDictionary.ContainsKey(targetToggle))
        {
            targetToggle.interactable = interactable;
            targetToggle.image.color = interactable ? Color.white : ToggleColors.k_DisabledColor;
        }
    }

    protected void EnableAllTogglesInteractability()
    {
        foreach (var toggleEntry in toggleDictionary)
        {
            ChangeToggleInteractability(toggleEntry.Key, true);
        }
    }

    protected void DisableAllTogglesInteractability()
    {
        foreach (var toggleEntry in toggleDictionary)
        {
            ChangeToggleInteractability(toggleEntry.Key, false);
        }
    }

    protected void EnableTogglesToAtLeast(T level)
    {
        foreach (var toggleEntry in toggleDictionary)
        {
            Toggle toggle = toggleEntry.Key;
            T type = toggleEntry.Value;
            bool shouldEnable = type.CompareTo(level) >= 0;
            ChangeToggleInteractability(toggle, shouldEnable);
        }
    }

    protected void EnableTogglesToAtMost(T level)
    {
        foreach (var toggleEntry in toggleDictionary)
        {
            Toggle toggle = toggleEntry.Key;
            T type = toggleEntry.Value;
            bool shouldEnable = type.CompareTo(level) <= 0;
            ChangeToggleInteractability(toggle, shouldEnable);
        }
    }

    public void Show()
    {
        gameObject.SetActive(true);
        ResetSelection();
    }

    public void Hide()
    {
        EnableAllTogglesInteractability();
        ResetSelection();
        gameObject.SetActive(false);
    }
}
