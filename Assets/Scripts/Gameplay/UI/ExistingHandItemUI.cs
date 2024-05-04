using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ExistingHandItemUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI m_existingHandText;
    [SerializeField] private TextMeshProUGUI m_playerThatPlayedHandText;
    [SerializeField] private TextMeshProUGUI m_roundHandWasPlayedText;

    public void GiveExistingHandItem(PokerHand hand, string playerName, int roundPlayed)
    {
        m_existingHandText.text = hand.GetStringRepresentation();
        m_playerThatPlayedHandText.text = playerName;
        m_roundHandWasPlayedText.text = roundPlayed == 0 ? "" : $"({roundPlayed})";
    }
}
