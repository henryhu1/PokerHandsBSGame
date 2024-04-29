using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayedHandLogItemUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI m_LogText;
    [SerializeField] private Image m_CorrectIcon;
    [SerializeField] private Image m_WrongIcon;
    private PlayedHandLogItem m_LogItem;

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

    public string GetPlayerWhoPlayedHand()
    {
        return m_LogItem.m_playerId;
    }

    public void SetTextColor(Color color)
    {
        m_LogText.color = color;
    }

    public void ShowHandPresentIcon(bool isCorrect)
    {
        m_CorrectIcon.enabled = isCorrect;
        m_WrongIcon.enabled = !isCorrect;
    }
}
