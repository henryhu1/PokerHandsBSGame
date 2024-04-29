using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AllOpponentCards : MonoBehaviour
{
    // TODO: programatically figure out how to evenly position opponent hands in space
    [SerializeField] private List<OpponentHand> opponentCardsGameObjects;

    private void Awake()
    {
        HideOpponentHands();
    }

    public void HideOpponentHands()
    {
        opponentCardsGameObjects.ForEach(i => i.gameObject.SetActive(false));
    }

    public void DisplayOpponentCards(List<int> opponentCardAmounts)
    {
        for (int i = 0; i < opponentCardsGameObjects.Count; i++)
        {
            if (i < opponentCardAmounts.Count)
            {
                opponentCardsGameObjects[i].gameObject.SetActive(true);
                opponentCardsGameObjects[i].DisplayCards(opponentCardAmounts[i]);
            }
            else
            {
                opponentCardsGameObjects[i].gameObject.SetActive(false);
            }
        }
    }
}
