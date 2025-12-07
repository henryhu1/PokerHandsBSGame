using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class OpponentHand : MonoBehaviour
{
    private static readonly List<int> s_cardRemovalOrder = new() { 1, 3, 2, 0, 4 };

    [SerializeField] private MeshRenderer m_meshRenderer;
    [SerializeField] private Material blankCardTexture;
    [SerializeField] private Material invisibleCardTexture;
    [SerializeField] private CardRegistrySO cardRegistry;

    private Vector3 showPlayerRotation = new(-90, 180, 0);

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

    private void DisplayMaterialsOnCards(Material[] cardMats, string playerName, ulong clientId)
    {
        int displayableCount = m_meshRenderer.materials.Length;
        Material[] replacingMats = m_meshRenderer.materials;
        int amount = cardMats.Length;

        m_opponentClientId = clientId;
        m_opponentName = playerName;
        m_opponentAmountOfCardsInHand = amount;
        int materialsToRemove = displayableCount - amount;
#if UNITY_EDITOR
        Debug.Log($"opponent has {amount} cards, removing {displayableCount} - {amount} = {materialsToRemove} materials (out of {m_meshRenderer.materials.Length})");
#endif
        for (int i = 0; i < displayableCount; i++)
        {
            if (i < materialsToRemove)
            {
                replacingMats[s_cardRemovalOrder[i]] = invisibleCardTexture;
            }
            else
            {
                replacingMats[s_cardRemovalOrder[i]] = cardMats[i - materialsToRemove];
            }
        }
        m_meshRenderer.materials = replacingMats;
    }

    private Material[] GetCardMaterials(List<Card> cards)
    {
        Material[] mats = new Material[cards.Count];
        for (int i = 0; i < cards.Count; i++)
        {
            Card atCard = cards[i];
            mats[i] = cardRegistry.GetEntry(atCard).material;
        }
        return mats;
    }

    public void DisplayCards(PlayerCardInfo opponentCardsInfo)
    {
        Material[] mats = GetCardMaterials(opponentCardsInfo.cards);
        DisplayMaterialsOnCards(mats, opponentCardsInfo.playerName, opponentCardsInfo.clientId);
        RevealHand();
    }

    public void DisplayBlanks(PlayerHiddenCardInfo hiddenCardInfo)
    {
        ConcealHand().OnComplete(() =>
        {
            int amount = hiddenCardInfo.amountOfCards;
            Material[] blankMaterials = new Material[amount];
            for (int i = 0; i < amount; i++)
            {
                blankMaterials[i] = blankCardTexture;
            }
            DisplayMaterialsOnCards(blankMaterials, hiddenCardInfo.playerName, hiddenCardInfo.clientId);
        });
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

    private Sequence RevealHand()
    {
        Sequence mySequence = DOTween.Sequence();
        mySequence.Insert(0, transform.DORotate(showPlayerRotation, k_rotationDuration));
        mySequence.Insert(0, transform.DOPunchPosition(Vector3.up * 0.05f, k_rotationDuration, 0, 0));
        return mySequence;
    }

    private Sequence ConcealHand()
    {
        Sequence mySequence = DOTween.Sequence();
        mySequence.Insert(0, transform.DORotate(m_originalRotation, k_rotationDuration));
        mySequence.Insert(0, transform.DOPunchPosition(Vector3.up * 0.05f, k_rotationDuration, 0, 0));
        return mySequence;
    }
}
