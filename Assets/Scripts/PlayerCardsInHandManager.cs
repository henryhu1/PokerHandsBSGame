using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class PlayerCardsInHandManager : MonoBehaviour
{
    public static PlayerCardsInHandManager Instance;

    [SerializeField] private GraphicRaycaster graphicRaycaster;
    [SerializeField] private CardRegistrySO cardRegistry;

    private readonly List<Draggable> cardObjects = new();
    private readonly List<Vector3> positions = new();
    private int cardDraggingIndex = -1;
    private int cardDraggingOriginalIndex = -1;
    private List<Card> cards = new();

    public bool areCardsSorted = false;

    private const float k_xSpacing = 0.06f;
    private const float k_yCenter = 1.1f;
    private const float k_ySpacing = 0.02f;
    private const float k_zCenter = 0.75f;
    private const float k_zSpacing = 0.001f;
    private const int k_maxCardsPerRow = 10;
    private const float k_cardMovementDuration = 0.2f;
    private static Vector3 k_cardZoom = Vector3.forward * 0.01f;
    private static Quaternion k_cardsFacePlayerRotation = new(90, 0, 0, 90);

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(Instance);
        }
        Instance = this;
    }

    // private void OnEnable()
    // {
    //     OrderCardsUI.Instance.OnOrderCards += OrderCardsUI_OrderCards;
    // }

    // private void OnDisable()
    // {
    //     OrderCardsUI.Instance.OnOrderCards -= OrderCardsUI_OrderCards;
    // }

    private void OrderCardsUI_OrderCards(bool isDoAscendingSort)
    {
        OrderCards(isDoAscendingSort);
    }

    public void DisplayPlayerCards(Card[] newCards)
    {
        ClearCards();
        cards = newCards.ToList();
        CreateCardObjects(cards);
    }

    public void ClearCards()
    {
        foreach (var c in cardObjects) Destroy(c.gameObject);
    }

    public void DestroyCardGameObjects()
    {
        foreach (Draggable cardGameObject in cardObjects) Destroy(cardGameObject.gameObject);
        cardObjects.Clear();
        positions.Clear();
        // areCardsSorted = false;
    }


    public void OrderCards(bool ascending)
    {
        DestroyCardGameObjects();
        cards.Sort();
        if (!ascending) cards.Reverse();
        DisplayPlayerCards(cards.ToArray());
        // areCardsSorted = true;
    }

    public void HandleCardDrag(Vector3 pos)
    {
        Vector3 slot;
        for (int i = 0; i < positions.Count; i++)
        {
            if (i == cardDraggingIndex) continue;
            slot = positions[i];
            if (Math.Abs(slot.x - pos.x) < 0.01 && Math.Abs(slot.y - pos.y) < 0.01)
            {
                Draggable draggingCard = cardObjects[cardDraggingIndex];
                Draggable displacedCard = cardObjects[i];
                Vector3 emptySpace = positions[cardDraggingIndex];
                StartCoroutine(MoveCard(displacedCard, emptySpace));
                cardObjects[cardDraggingIndex] = displacedCard;
                displacedCard.Index = cardDraggingIndex;
                cardObjects[i] = draggingCard;
                draggingCard.Index = i;
                cardDraggingIndex = i;
            }
        }
    }

    public void HandleCardEndDrag(int index)
    {
        if (cardDraggingIndex != index) return;
        Draggable exitedCard = cardObjects[index];
        StartCoroutine(MoveCard(exitedCard, positions[index], k_cardMovementDuration, true));
        if (cardDraggingOriginalIndex != index)
        {
            areCardsSorted = false;
        }
        graphicRaycaster.enabled = true;
    }

    public void HandleCardEnter(int index)
    {
        if (cardDraggingIndex != -1) return;
        Draggable enteredCard = cardObjects[index];
        StartCoroutine(MoveCard(enteredCard, positions[index] + k_cardZoom, k_cardMovementDuration));
    }

    public void HandleCardExit(int index)
    {
        if (cardDraggingIndex != -1 || cardDraggingIndex == index) return;
        Draggable exitedCard = cardObjects[index];
        StartCoroutine(MoveCard(exitedCard, positions[index], k_cardMovementDuration));
    }

    public void SetCardEmptySlotPosition(Draggable dragging)
    {
        cardDraggingIndex = dragging.Index;
        cardDraggingOriginalIndex = dragging.Index;
        graphicRaycaster.enabled = false;
    }

    private IEnumerator MoveCard(Draggable card, Vector3 finalPosition, float duration = k_cardMovementDuration, bool isEndDrag = false)
    {
        Vector3 start = card.transform.position;
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;

            float step = time / duration;
            //float curveStep = movementCurve.Evaluate(step);
            card.transform.position = Vector3.Lerp(start, finalPosition, step);

            yield return null;
        }

        card.transform.position = finalPosition;
        if (isEndDrag) cardDraggingIndex = -1;
    }

    public void CreateCardObjects(List<Card> myCards)
    {
        int numberOfCardsToDisplay = myCards.Count;
        int rowsOfCards = numberOfCardsToDisplay / k_maxCardsPerRow;
        int cardsPerRow = Math.Min(numberOfCardsToDisplay, k_maxCardsPerRow);
        float xPos = -cardsPerRow * k_xSpacing / 2;
        float yPos = k_yCenter - (rowsOfCards - 1) * k_ySpacing / 2;
        float zPos = k_zCenter - (rowsOfCards - 1) * k_zSpacing / 2;
        for (int i = 0; i < numberOfCardsToDisplay; i++)
        {
            Card currentCard = myCards[i];
            int cardsNowInRow = i % cardsPerRow;
            if (cardsNowInRow == 0)
            {
                xPos = -Math.Min(numberOfCardsToDisplay - i, k_maxCardsPerRow) * k_xSpacing / 2;
                yPos += k_ySpacing;
                zPos -= k_zSpacing;
            }
            Vector3 cardSlot = new(xPos, yPos, zPos);
            positions.Add(cardSlot);
            GameObject cardPrefab = cardRegistry.GetPrefab(currentCard.Rank, currentCard.Suit);
            GameObject cardGameObject = Instantiate(cardPrefab, cardSlot, k_cardsFacePlayerRotation);
            Draggable draggable = cardGameObject.GetComponent<Draggable>();
            draggable.Index = cardObjects.Count;
            xPos += k_xSpacing;
            cardObjects.Add(draggable);
        }
    }
}
