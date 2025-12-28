using TMPro;
using UnityEngine;

public class PlayedHandLogItemUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI placementText;
    [SerializeField] private TextMeshProUGUI logText;
    [SerializeField] private GameObject correctIcon;
    [SerializeField] private GameObject wrongIcon;
    private PlayedHandLogItem logItem;

    public static Color normalTextColor = Color.white;
    public static Color highlightedTextColor = new(1, 1, 0.5f);

    private void Awake()
    {
        ResetIcons();
    }

    public void ResetIcons()
    {
        correctIcon.SetActive(false);
        wrongIcon.SetActive(false);
    }

    public void GiveLogItem(int place, PlayedHandLogItem playedHandLogItem)
    {
        placementText.text = $"({place})";
        logItem = playedHandLogItem;
        logText.text = playedHandLogItem.m_playerName + ": " + playedHandLogItem.m_playedHand.GetStringRepresentation();
    }

    public ulong GetClientIDWhoPlayedHand()
    {
        return logItem.m_clientId;
    }

    public string GetNameWhoPlayedHand()
    {
        return logItem.m_playerName;
    }

    public PokerHand GetPlayedHand()
    {
        return logItem.m_playedHand;
    }

    public void SetNormalTextColor()
    {
        logText.color = normalTextColor;
    }

    public void SetHighlightedTextColor()
    {
        logText.color = highlightedTextColor;
    }

    public void SetTextAlphaColor(float a)
    {
        Color textColor = logText.color;
        textColor.a = a;
        logText.color = textColor;
    }

    public void ShowHandPresentIcon(bool isCorrect)
    {
        correctIcon.SetActive(isCorrect);
        wrongIcon.SetActive(!isCorrect);
    }
}
