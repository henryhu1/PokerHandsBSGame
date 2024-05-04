using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ResultItemUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI m_placementText;
    [SerializeField] private TextMeshProUGUI m_playerNameText;

    public void GivePlacementItem(int placement, string name)
    {
        m_placementText.text = $"{placement})";
        m_placementText.text = name;
    }
}
