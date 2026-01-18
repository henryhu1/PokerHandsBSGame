using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class ExistingHandItemUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private TextMeshProUGUI m_existingHandText;
    [SerializeField] private TextMeshProUGUI m_playerThatPlayedHandText;
    [SerializeField] private TextMeshProUGUI m_roundHandWasPlayedText;

    [Header("Firing Events")]
    [SerializeField] private PokerHandEventChannelSO OnPreviewPokerHand;
    [SerializeField] private VoidEventChannelSO OnStopPreview;

    private PokerHand existingHand;

    public void GiveExistingHandItem(PokerHand hand, string playerName, int roundPlayed)
    {
        existingHand = hand;
        m_existingHandText.text = hand.GetStringRepresentation();
        m_playerThatPlayedHandText.text = playerName;
        m_roundHandWasPlayedText.text = roundPlayed == 0 ? "" : $"({roundPlayed})";
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        OnPreviewPokerHand.RaiseEvent(existingHand);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        OnStopPreview.RaiseEvent();
    }
}
