using System.Collections.Generic;
using UnityEngine;

public class AllOpponentCards : MonoBehaviour
{
    public static AllOpponentCards Instance { get; private set; }

    // TODO: programatically figure out how to evenly position opponent hands in space
    [SerializeField] private List<OpponentHand> opponentCardsGameObjects;

    [HideInInspector]
    public delegate void SelectOpponentHandDelegateHandler(ulong clientId);
    [HideInInspector]
    public event SelectOpponentHandDelegateHandler OnSelectOpponentHand;

    [HideInInspector]
    public delegate void UnselectAllOpponentHandDelegateHandler();
    [HideInInspector]
    public event UnselectAllOpponentHandDelegateHandler OnUnselectAllOpponentHand;

    [HideInInspector]
    public delegate void MouseEnterOpponentHandDelegateHandler(ulong clientId, string name, int amountOfCards);
    [HideInInspector]
    public event MouseEnterOpponentHandDelegateHandler OnMouseEnterOpponentHand;

    [HideInInspector]
    public delegate void MouseExitOpponentHandDelegateHandler();
    [HideInInspector]
    public event MouseExitOpponentHandDelegateHandler OnMouseExitOpponentHand;

    private OpponentHand m_userSelectedHand;
    public OpponentHand UserSelectedHand { get { return m_userSelectedHand; } }

    private void Awake()
    {
        if (Instance != this && Instance != null)
        {
            Destroy(Instance.gameObject);
        }
        Instance = this;

        HideOpponentHands();
    }

    private void Start()
    {
        opponentCardsGameObjects.ForEach(i =>
        {
            i.OnMouseEnterThisHand += (ulong clientId, string name, int amountOfCards) =>
            {
                OnMouseEnterOpponentHand?.Invoke(clientId, name, amountOfCards);
            };
            i.OnMouseExitThisHand += () =>
            {
                OnMouseExitOpponentHand?.Invoke();
            };
            i.OnSelectedThisHand += () =>
            {
                SelectOpponentHand(i);
            };
        });
    }

    public void HideOpponentHands()
    {
        opponentCardsGameObjects.ForEach(i => i.gameObject.SetActive(false));
    }

    public void DisplayOpponentCards(List<List<Card>> opponentCards, List<string> opponentNames, ulong[] clientIds)
    {
        for (int i = 0; i < opponentCardsGameObjects.Count; i++)
        {
            if (i < opponentCards.Count && opponentCards[i].Count > 0)
            {
                opponentCardsGameObjects[i].gameObject.SetActive(true);
                opponentCardsGameObjects[i].DisplayCards(opponentCards[i], opponentNames[i], clientIds[i]);
            }
            else
            {
                opponentCardsGameObjects[i].gameObject.SetActive(false);
            }
        }
    }

    public ulong GetSelectedHandsClientId()
    {
        return m_userSelectedHand.OpponentClientId;
    }

    private void SelectOpponentHand(OpponentHand opponentHand)
    {
        m_userSelectedHand = opponentHand;
        OnSelectOpponentHand?.Invoke(opponentHand.OpponentClientId);
    }

    public void UnselectAllOpponentHands()
    {
        m_userSelectedHand = null;
        OnUnselectAllOpponentHand?.Invoke();
    }
}
