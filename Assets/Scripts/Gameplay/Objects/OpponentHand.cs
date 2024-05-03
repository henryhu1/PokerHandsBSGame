using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpponentHand : MonoBehaviour
{
    private static List<int> s_cardRemovalOrder = new List<int> { 1, 3, 2, 0, 4 };

    [SerializeField] private MeshRenderer m_meshRenderer;
    [SerializeField] private Material blankCardTexture;
    private ulong m_opponentClientId;
    public ulong OpponentClientId { get { return m_opponentClientId; } }
    private string m_opponentName;
    public string OpponentName { get { return m_opponentName; } }
    private int m_opponentAmountOfCardsInHand;

    [HideInInspector]
    public delegate void MouseEnterThisHandDelegateHandler(ulong clientId, string name, int amountOfCards);
    [HideInInspector]
    public event MouseEnterThisHandDelegateHandler OnMouseEnterThisHand;

    [HideInInspector]
    public delegate void MouseExitThisHandDelegateHandler();
    [HideInInspector]
    public event MouseExitThisHandDelegateHandler OnMouseExitThisHand;

    [HideInInspector]
    public delegate void SelectedThisHandDelegateHandler();
    [HideInInspector]
    public event SelectedThisHandDelegateHandler OnSelectedThisHand;

    public void DisplayCards(int amount, string name, ulong clientId)
    {
        m_opponentClientId = clientId;
        m_opponentName = name;
        m_opponentAmountOfCardsInHand = amount;
        int materialsToRemove = m_meshRenderer.materials.Length - amount;
        Debug.Log($"opponent has {amount} cards, removing {materialsToRemove} materials");
        Material[] mats = m_meshRenderer.materials;
        for (int i = 0; i < m_meshRenderer.materials.Length; i++)
        {
            if (i < materialsToRemove)
            {
                mats[s_cardRemovalOrder[i]] = null;
            }
            else
            {
                mats[s_cardRemovalOrder[i]] = blankCardTexture;
            }
        }
        m_meshRenderer.materials = mats;
    }

    private void OnMouseEnter()
    {
        OnMouseEnterThisHand?.Invoke(m_opponentClientId, m_opponentName, m_opponentAmountOfCardsInHand);
    }

    private void OnMouseExit()
    {
        OnMouseExitThisHand?.Invoke();
    }

    public void SetSelectedHand()
    {
        OnSelectedThisHand?.Invoke();
    }
}
