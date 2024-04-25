using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayedHandLogUI : MonoBehaviour
{
    [SerializeField] private PlayedHandLogItemUI m_PlayedHandLogItemPrefab;
    [SerializeField] private GameObject m_LogContent;

    private List<PlayedHandLogItemUI> m_PlayedHandLogItems;

    private static Color s_CardLogItemColor = Color.white;
    private static Color s_CardLogItemHighlightedColor = Color.yellow; // new Color(1, 1, 0.5f);

    private void Awake()
    {
        m_PlayedHandLogItems = new List<PlayedHandLogItemUI>();
    }

    private void Start()
    {
        PokerHandsBullshitGame.Instance.OnAddToCardLog += GameManager_AddToCardLog;
        PokerHandsBullshitGame.Instance.OnClearCardLog += GameManager_ClearCardLog;
    }

    private void GameManager_AddToCardLog(ulong playerId, string playerName, PokerHand playedHand)
    {
        PlayedHandLogItemUI cardLogItem = Instantiate(m_PlayedHandLogItemPrefab, m_LogContent.transform);
        cardLogItem.PlayerOwnerID = playerId;
        cardLogItem.PlayerOwnerName = playerName;
        cardLogItem.PlayedHand = playedHand.GetStringRepresentation();

        m_PlayedHandLogItems.Add(cardLogItem);
    }

    private void GameManager_ClearCardLog()
    {
        foreach (PlayedHandLogItemUI playedHandLogItemUI in m_PlayedHandLogItems) Destroy(playedHandLogItemUI.gameObject);
        m_PlayedHandLogItems.Clear();
    }

    private void GameManager_RoundEnd()
    {
        m_PlayedHandLogItems.ForEach(x => Destroy(x));
        m_PlayedHandLogItems.Clear();
    }

    public void HighlightPlayersPlayedHands(ulong playerID)
    {
        foreach (PlayedHandLogItemUI playedHandLogItem in m_PlayedHandLogItems) {
            if (playedHandLogItem.PlayerOwnerID == playerID)
            {
                playedHandLogItem.SetTextColor(s_CardLogItemHighlightedColor);
            }
            else
            {
                playedHandLogItem.SetTextColor(s_CardLogItemColor);
            }
        }
    }
}
