using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayedHandLogItemUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI m_LogText;
    [SerializeField] private Image m_CorrectIcon;
    [SerializeField] private Image m_WrongIcon;
    private PlayedHandLogItem m_LogItem;

    public static Color s_NormalTextColor = Color.white;
    public static Color s_HighlightedTextColor = new Color(1, 1, 0.5f);

    private void Awake()
    {
        ResetIcons();
    }

    public void ResetIcons()
    {
        m_CorrectIcon.enabled = false;
        m_WrongIcon.enabled = false;
    }

    public void GiveLogItem(PlayedHandLogItem playedHandLogItem)
    {
        m_LogItem = playedHandLogItem;
        m_LogText.text = playedHandLogItem.m_playerName + ": " + playedHandLogItem.m_playedHand.GetStringRepresentation();
    }

    public ulong GetClientIDWhoPlayedHand()
    {
        return m_LogItem.m_clientId;
    }

    public string GetNameWhoPlayedHand()
    {
        return m_LogItem.m_playerName;
    }

    public PokerHand GetPlayedHand()
    {
        return m_LogItem.m_playedHand;
    }

    public void SetNormalTextColor()
    {
        m_LogText.color = s_NormalTextColor;
    }

    public void SetHighlightedTextColor()
    {
        m_LogText.color = s_HighlightedTextColor;
    }

    public void SetTextAlphaColor(float a)
    {
        Color textColor = m_LogText.color;
        textColor.a = a;
        m_LogText.color = textColor;
    }

    public void ShowHandPresentIcon(bool isCorrect)
    {
        m_CorrectIcon.enabled = isCorrect;
        m_WrongIcon.enabled = !isCorrect;
    }
}
