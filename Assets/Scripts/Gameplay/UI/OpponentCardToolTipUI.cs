using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class OpponentCardToolTipUI : MonoBehaviour
{
    private static Vector2 s_offset = new Vector2(0f, -30.0f);
    [SerializeField] private Canvas m_canvas;
    [SerializeField] private TextMeshProUGUI m_playerNameText;
    [SerializeField] private TextMeshProUGUI m_cardAmountText;

    private void Start()
    {
        AllOpponentCards.Instance.OnMouseEnterOpponentHand += Show;
        AllOpponentCards.Instance.OnMouseExitOpponentHand += Hide;

        Hide();
    }

    private void Update()
    {
        Vector2 pos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(m_canvas.transform as RectTransform, Input.mousePosition, m_canvas.worldCamera, out pos);
        pos += s_offset;
        transform.position = m_canvas.transform.TransformPoint(pos);
    }

    public void Show(ulong clientId, string name, int amountOfCards)
    {
        gameObject.SetActive(true);
        m_playerNameText.text = name;
        string cardAmountTextSuffix = amountOfCards == 1 ? "card" : "cards";
        m_cardAmountText.text = $"{amountOfCards} {cardAmountTextSuffix}";
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
