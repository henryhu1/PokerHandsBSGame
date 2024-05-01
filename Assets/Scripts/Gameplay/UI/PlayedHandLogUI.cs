using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayedHandLogUI : MonoBehaviour
{
    [SerializeField] private PlayedHandLogItemUI m_PlayedHandLogItemUIPrefab;
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
        PokerHandsBullshitGame.Instance.OnEndOfRound += GameManager_EndOfRound;
        PokerHandsBullshitGame.Instance.OnClearCardLog += GameManager_ClearCardLog;
    }

    private void GameManager_AddToCardLog(PlayedHandLogItem playedHandLogItem)
    {
        PlayedHandLogItemUI cardLogItem = Instantiate(m_PlayedHandLogItemUIPrefab, m_LogContent.transform);
        cardLogItem.GiveLogItem(playedHandLogItem);

        m_PlayedHandLogItems.Add(cardLogItem);
    }

    private void GameManager_EndOfRound(List<bool> playedHandsPresent, List<PokerHand> _)
    {
        for (int i = 0; i < playedHandsPresent.Count; i++)
        {
            m_PlayedHandLogItems[i].ShowHandPresentIcon(playedHandsPresent[i]);
        }
    }

    private void GameManager_ClearCardLog()
    {
        foreach (PlayedHandLogItemUI playedHandLogItemUI in m_PlayedHandLogItems) Destroy(playedHandLogItemUI.gameObject);
        m_PlayedHandLogItems.Clear();
    }

    // TODO:
    public void HighlightPlayersPlayedHands(ulong clientId, string playerId)
    {
        foreach (PlayedHandLogItemUI playedHandLogItem in m_PlayedHandLogItems) {
            if (playedHandLogItem.GetPlayerWhoPlayedHand() == playerId)
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
