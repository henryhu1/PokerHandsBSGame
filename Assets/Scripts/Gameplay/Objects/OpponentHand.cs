using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class OpponentHand : MonoBehaviour
{
    private static List<int> s_cardRemovalOrder = new List<int> { 1, 3, 2, 0, 4 };

    [SerializeField] private MeshRenderer m_meshRenderer;
    [SerializeField] private Material blankCardTexture;
    [SerializeField] private CardRegistrySO cardRegistry;

    private ulong m_opponentClientId;
    public ulong OpponentClientId { get { return m_opponentClientId; } }
    private string m_opponentName;
    public string OpponentName { get { return m_opponentName; } }
    private int m_opponentAmountOfCardsInHand;

    private Vector3 m_originalRotation;
    const float k_rotationDuration = 0.5f;

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

    private void Start()
    {
        m_originalRotation = transform.rotation.eulerAngles;
    }

    public void DisplayCards(List<Card> cards, string name, ulong clientId)
    {
        int amount = cards.Count;

        m_opponentClientId = clientId;
        m_opponentName = name;
        m_opponentAmountOfCardsInHand = amount;
        int materialsToRemove = m_meshRenderer.materials.Length - amount;
#if UNITY_EDITOR
        Debug.Log($"opponent has {amount} cards, removing {materialsToRemove} materials");
#endif
        Material[] mats = m_meshRenderer.materials;
        for (int i = 0; i < m_meshRenderer.materials.Length; i++)
        {
            if (i < materialsToRemove)
            {
                mats[s_cardRemovalOrder[i]] = null;
            }
            else
            {
                Card atCard = cards[i - materialsToRemove];
                mats[s_cardRemovalOrder[i]] = cardRegistry.GetEntry(atCard).material;
            }
        }
        m_meshRenderer.materials = mats;
        gameObject.SetActive(m_meshRenderer.materials.Length > 0);
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

    private void RevealHand()
    {
        var rot = transform.rotation.eulerAngles;
        Vector3 newRotation = new(m_originalRotation.x, -1 * m_originalRotation.y, m_originalRotation.z);
        transform.DORotate(newRotation, k_rotationDuration);
    }

    private void ConcealHand()
    {
        transform.DORotate(m_originalRotation, k_rotationDuration);
    }
}
