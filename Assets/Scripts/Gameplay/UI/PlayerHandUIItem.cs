using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHandUIItem : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI playerNameText;
    [SerializeField] private RawImage[] cardSlots;

    public void ShowCardImages(string playerName, List<Texture2D> images)
    {
        playerNameText.text = playerName;
        int cardCount = images.Count();
        for (int i = 0; i < cardSlots.Count(); i++)
        {
            RawImage cardSlot = cardSlots[i];
            if (i < cardCount)
            {
                cardSlot.texture = images[i];
                cardSlots[i].gameObject.SetActive(true);
            }
            else
            {
                cardSlots[i].gameObject.SetActive(false);
            }
        }
    }
}
