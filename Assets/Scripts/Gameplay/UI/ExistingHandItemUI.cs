using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ExistingHandItemUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI m_existingHandText;
    [SerializeField] private TextMeshProUGUI m_playerThatPlayedHandText;

    private void Awake()
    {
        m_playerThatPlayedHandText.text = "";
    }

    // TODO: if existing hand was played, have the player's name displayed
    //   Also change existing hand text wrapping to enabled and adjust size for items
    public void GiveExistingHandItem(PokerHand hand) // , string playerName)
    {
        m_existingHandText.text = hand.GetStringRepresentation();
        // m_playerThatPlayedHandText.text = playerName;
    }
}
