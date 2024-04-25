using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToggleColors
{
    private static Color k_ToggleColorOn = new Color(1, 1, 0.5f);
    public static Color k_DisabledColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);

    private Color m_ToggleColorNormal;
    public Color ToggleColorNormal
    {
        get { return m_ToggleColorNormal; }
        set { m_ToggleColorNormal = value; }
    }
    private Color m_ToggleColorHighlighted;
    public Color ToggleColorHighlighted
    {
        get { return m_ToggleColorHighlighted; }
        set { m_ToggleColorHighlighted = value; }
    }
    private Color m_ToggleColorSelected;
    public Color ToggleColorSelected
    {
        get { return m_ToggleColorSelected; }
        set { m_ToggleColorSelected = value; }
    }
    private Color m_ToggleColorPressed;
    public Color ToggleColorPressed
    {
        get { return m_ToggleColorPressed; }
        set { m_ToggleColorPressed = value; }
    }

    public ToggleColors()
    {
        m_ToggleColorNormal = k_ToggleColorOn;
        m_ToggleColorHighlighted = k_ToggleColorOn;
        m_ToggleColorSelected = k_ToggleColorOn;
        m_ToggleColorPressed = k_ToggleColorOn;
    }

    public void ChangeToggleColors()
    {
        m_ToggleColorNormal = k_ToggleColorOn;
        m_ToggleColorHighlighted = k_ToggleColorOn;
        m_ToggleColorSelected = k_ToggleColorOn;
        m_ToggleColorPressed = k_ToggleColorOn;
    }

    public void ChangeToggleColors(ColorBlock colorBlock)
    {
        m_ToggleColorNormal = colorBlock.normalColor;
        m_ToggleColorHighlighted = colorBlock.highlightedColor;
        m_ToggleColorSelected = colorBlock.selectedColor;
        m_ToggleColorPressed = colorBlock.pressedColor;
    }
}
