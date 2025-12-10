using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CardsInRoundUI : MonoBehaviour
{
    [Header("Card Registry")]
    [SerializeField] private CardRegistrySO cardRegistry;

    [Header("UI")]
    [SerializeField] private PlayerHandUIItem playerHandPrefab;
    [SerializeField] private GameObject scrollContent;

    private TransitionableUIBase animatable;

    [Header("Listening Events")]
    [SerializeField] private PlayerCardInfoListEventChannelSO OnRevealAllCards;
    [SerializeField] private VoidEventChannelSO OnNextRoundStarting;
    [SerializeField] private VoidEventChannelSO OnGameWon;

    private List<PlayerHandUIItem> playerHands = new();

    private void Awake()
    {
        animatable = GetComponent<TransitionableUIBase>();
    }

    private void OnEnable()
    {
        OnRevealAllCards.OnEventRaised += FillCardsInRound;
        OnNextRoundStarting.OnEventRaised += MoveOffScreen;
        OnGameWon.OnEventRaised += MoveOffScreen;
    }

    private void OnDisable()
    {
        OnRevealAllCards.OnEventRaised -= FillCardsInRound;
        OnNextRoundStarting.OnEventRaised -= MoveOffScreen;
        OnGameWon.OnEventRaised -= MoveOffScreen;
    }

    private void MoveOffScreen()
    {
        if (!animatable.IsOffScreen()) animatable.StartAnimation();
    }

    private void FillCardsInRound(List<PlayerCardInfo> allCardInfo)
    {
        int uiItems = 0;
        while (uiItems < allCardInfo.Count())
        {
            PlayerCardInfo info = allCardInfo[uiItems];
            List<Texture2D> cardImages = info.cards.Select(card => cardRegistry.GetEntry(card).texture).ToList();
            PlayerHandUIItem playerHand;
            if (uiItems < playerHands.Count())
            {
                playerHand = playerHands[uiItems];
                playerHand.gameObject.SetActive(true);
            }
            else
            {
                playerHand = Instantiate(playerHandPrefab, scrollContent.transform);
                playerHands.Add(playerHand);
            }
            playerHand.ShowCardImages(info.playerName, cardImages);
            uiItems++;
        }

        while (uiItems < playerHands.Count())
        {
            playerHands[uiItems].gameObject.SetActive(false);
            uiItems++;
        }

        animatable.StartAnimation();
    }
}
