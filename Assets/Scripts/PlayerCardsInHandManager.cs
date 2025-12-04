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

    [Header("Listening Events")]
    [SerializeField] private VoidEventChannelSO OnOrderCards;

    private readonly List<(Draggable, int)> cardObjects = new();
    private readonly List<Vector3> positions = new();
    private int cardDraggingIndex = -1;
    private int cardDraggingOriginalIndex = -1;

    public CardSortState cardSortState = CardSortState.UNSORTED;

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

    private void OnEnable()
    {
        OnOrderCards.OnEventRaised += OrderCardsUI_OrderCards;
    }

    private void OnDisable()
    {
        OnOrderCards.OnEventRaised -= OrderCardsUI_OrderCards;
    }

    private void OrderCardsUI_OrderCards()
    {
        OrderCards(cardSortState != CardSortState.ASCENDING);
    }

    private void UpdateCardDisplay()
    {
        for (int i = 0; i < cardObjects.Count; i++)
        {
            Draggable cardObject = cardObjects[i].Item1;
            cardObject.transform.position = positions[i];
        }
    }

    public void DestroyCardGameObjects()
    {
        foreach ((Draggable cardGameObject, int _) in cardObjects) Destroy(cardGameObject.gameObject);
        cardObjects.Clear();
        positions.Clear();
        cardSortState = CardSortState.UNSORTED;
    }

    public void OrderCards(bool ascending)
    {
        int order = ascending ? 1 : -1;
        cardObjects.Sort((a, b) => a.Item2 < b.Item2 ? order : -order);
        UpdateCardDisplay();
        cardSortState = ascending ? CardSortState.ASCENDING : CardSortState.DESCENDING;
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
                (Draggable draggingCard, int draggingSortIndex) = cardObjects[cardDraggingIndex];
                (Draggable displacedCard, int displacedSortIndex) = cardObjects[i];
                Vector3 emptySpace = positions[cardDraggingIndex];
                StartCoroutine(MoveCard(displacedCard, emptySpace));
                cardObjects[cardDraggingIndex] = (displacedCard, displacedSortIndex);
                cardObjects[i] = (draggingCard, draggingSortIndex);
                cardDraggingIndex = i;
            }
        }
    }

    public void HandleCardEndDrag(Draggable wasDragging)
    {
        int index = cardObjects.FindIndex(0, (cardSortIndexPair) => cardSortIndexPair.Item1 == wasDragging);
        if (index == -1 || cardDraggingIndex != index) return;

        StartCoroutine(MoveCard(cardObjects[index].Item1, positions[index], isEndDrag: true));

        if (cardDraggingOriginalIndex != index)
        {
            cardSortState = CardSortState.UNSORTED;
        }
        graphicRaycaster.enabled = true;
    }

    public void HandleCardEnter(Draggable mayDrag)
    {
        if (cardDraggingIndex != -1) return;

        int index = cardObjects.FindIndex(0, (cardSortIndexPair) => cardSortIndexPair.Item1 == mayDrag);
        if (index == -1) return;

        StartCoroutine(MoveCard(cardObjects[index].Item1, positions[index] + k_cardZoom));
    }

    public void HandleCardExit(Draggable didNotDrag)
    {
        if (cardDraggingIndex != -1) return;

        int index = cardObjects.FindIndex(0, (cardSortIndexPair) => cardSortIndexPair.Item1 == didNotDrag);
        if (index == -1) return;

        StartCoroutine(MoveCard(cardObjects[index].Item1, positions[index]));
    }

    public void HandleCardStartDrag(Draggable dragging)
    {
        if (cardDraggingIndex != -1) return;

        int index = cardObjects.FindIndex(0, (cardSortIndexPair) => cardSortIndexPair.Item1 == dragging);
        if (index == -1) return;

        cardDraggingIndex = index;
        cardDraggingOriginalIndex = index;
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
            card.transform.position = Vector3.Lerp(start, finalPosition, step);

            yield return null;
        }

        card.transform.position = finalPosition;
        if (isEndDrag) cardDraggingIndex = -1;
    }

    public void CreateCardObjects(List<Card> myCards)
    {
        List<Card> cardsSorted = new(myCards);
        cardsSorted.Sort();
        int[] cardIndexInPosition = myCards.Select(card => cardsSorted.IndexOf(card)).ToArray();

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
            xPos += k_xSpacing;
            cardObjects.Add((draggable, cardIndexInPosition[i]));
        }
    }
}
