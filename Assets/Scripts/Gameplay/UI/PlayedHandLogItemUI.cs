using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayedHandLogItemUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI m_LogText;
    private ulong m_PlayerOwnerID;
    public ulong PlayerOwnerID
    {
        get { return m_PlayerOwnerID; }
        set { m_PlayerOwnerID = value; }
    }
    private string m_PlayerOwnerName;
    public string PlayerOwnerName
    {
        get { return m_PlayerOwnerName; }
        set {
            m_PlayerOwnerName = value;
            m_LogText.text = m_PlayerOwnerName + ": " + m_PlayedHand;
        }
    }
    private string m_PlayedHand;
    public string PlayedHand
    {
        get { return m_PlayedHand; }
        set
        {
            m_PlayedHand = value;
            m_LogText.text = m_PlayerOwnerName + ": " + m_PlayedHand;
        }
    }

    public void SetTextColor(Color color)
    {
        m_LogText.color = color;
    }
}
