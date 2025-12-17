using UnityEngine;
using UnityEngine.UI;

public class ToggleColors
{
    private static Color k_ToggleColorOn = new(1, 1, 0.5f);
    public static Color k_DisabledColor = new(0.5f, 0.5f, 0.5f, 0.5f);

    public Color toggleColorNormal;
    public Color toggleColorHighlighted;
    public Color toggleColorSelected;
    public Color toggleColorPressed;

    public ToggleColors()
    {
        toggleColorNormal = k_ToggleColorOn;
        toggleColorHighlighted = k_ToggleColorOn;
        toggleColorSelected = k_ToggleColorOn;
        toggleColorPressed = k_ToggleColorOn;
    }

    public void ChangeToggleColors()
    {
        toggleColorNormal = k_ToggleColorOn;
        toggleColorHighlighted = k_ToggleColorOn;
        toggleColorSelected = k_ToggleColorOn;
        toggleColorPressed = k_ToggleColorOn;
    }

    public void ChangeToggleColors(ColorBlock colorBlock)
    {
        toggleColorNormal = colorBlock.normalColor;
        toggleColorHighlighted = colorBlock.highlightedColor;
        toggleColorSelected = colorBlock.selectedColor;
        toggleColorPressed = colorBlock.pressedColor;
    }
}
