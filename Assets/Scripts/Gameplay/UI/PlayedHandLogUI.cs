using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayedHandLogUI : MonoBehaviour
{
    public static PlayedHandLogUI Instance { get; private set; }

    [SerializeField] private PlayedHandLogItemUI m_PlayedHandLogItemUIPrefab;
    [SerializeField] private GameObject m_LogContent;

    // TODO: GetPlayerAndRoundOfPlayedHand and AllOpponentCards events must iterate over this list
    //   to find the corresponding played hands, a better data structure could be used for better efficiency
    private List<PlayedHandLogItemUI> m_PlayedHandLogItems;

    private const float k_pulseCycle = 0.5f;
    private Coroutine m_PulsingCoroutine;

    private void Awake()
    {
        Instance = this;

        m_PlayedHandLogItems = new List<PlayedHandLogItemUI>();
    }

    private void Start()
    {
        AllOpponentCards.Instance.OnSelectOpponentHand += AllOpponentCards_SelectOpponentHand;
        AllOpponentCards.Instance.OnUnselectAllOpponentHand += AllOpponentCards_UnselectAllOpponentHand;
        AllOpponentCards.Instance.OnMouseEnterOpponentHand += AllOpponentCards_MouseEnterOpponentHand;
        AllOpponentCards.Instance.OnMouseExitOpponentHand += AllOpponentCards_MouseExitOpponentHand;
        PokerHandsBullshitGame.Instance.OnAddToCardLog += GameManager_AddToCardLog;
        PokerHandsBullshitGame.Instance.OnEndOfRound += GameManager_EndOfRound;
        PokerHandsBullshitGame.Instance.OnClearCardLog += GameManager_ClearCardLog;
    }

    private void AllOpponentCards_SelectOpponentHand(ulong clientId)
    {
        HighlightPlayersPlayedHands(clientId);
    }

    private void AllOpponentCards_UnselectAllOpponentHand()
    {
        UnhighlightPlayedHands();
    }

    private void AllOpponentCards_MouseEnterOpponentHand(ulong clientId, string name, int amountOfCards)
    {
        StartPulsingAnimation(clientId);
    }

    private void AllOpponentCards_MouseExitOpponentHand()
    {
        if (AllOpponentCards.Instance.UserSelectedHand != null)
        {
            HighlightPlayersPlayedHands(AllOpponentCards.Instance.GetSelectedHandsClientId());
        }
        else
        {
            UnhighlightPlayedHands();
        }
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

    public (string, int) GetPlayerAndRoundOfPlayedHand(PokerHand hand)
    {
        for (int i = 0; i < m_PlayedHandLogItems.Count; i++)
        {
            PlayedHandLogItemUI logItem = m_PlayedHandLogItems[i];
            if (hand.CompareTo(logItem.GetPlayedHand()) == 0)
            {
                return (logItem.GetNameWhoPlayedHand(), i);
            }
        }
        return (null, -1);
    }

    private void StartPulsingAnimation(ulong clientId)
    {
        m_PulsingCoroutine = StartCoroutine(PulsingAnimation(clientId));
    }

    private void StopPulsingAnimation()
    {
        if (m_PulsingCoroutine != null)
        {
            StopCoroutine(m_PulsingCoroutine);
        }
        m_PulsingCoroutine = null;
    }

    private IEnumerator PulsingAnimation(ulong clientId)
    {
        List<PlayedHandLogItemUI> pulsingText = new List<PlayedHandLogItemUI>();
        foreach (PlayedHandLogItemUI playedHandLogItem in m_PlayedHandLogItems)
        {
            if (playedHandLogItem.GetClientIDWhoPlayedHand() == clientId)
            {
                playedHandLogItem.SetHighlightedTextColor();
                pulsingText.Add(playedHandLogItem);
            }
        }

        float time = k_pulseCycle;
        bool isIncreasing = false;

        while (true)
        {
            if (isIncreasing)
            {
                time += Time.deltaTime;
                if (time >= k_pulseCycle)
                {
                    isIncreasing = false;
                    time = k_pulseCycle;
                }
            }
            else
            {
                time -= Time.deltaTime;
                if (time <= 0)
                {
                    isIncreasing = true;
                    time = 0f;
                }
            }
            float alpha = Mathf.Lerp(0f, 1f, time / k_pulseCycle);
            foreach (PlayedHandLogItemUI pulsers in pulsingText)
            {
                pulsers.SetTextAlphaColor(alpha);
            }
            yield return null;
        }
    }

    private void HighlightPlayersPlayedHands(ulong clientId)
    {
        StopPulsingAnimation();
        foreach (PlayedHandLogItemUI playedHandLogItem in m_PlayedHandLogItems)
        {
            if (playedHandLogItem.GetClientIDWhoPlayedHand() == clientId)
            {
                playedHandLogItem.SetHighlightedTextColor();
            }
            else
            {
                playedHandLogItem.SetNormalTextColor();
            }
        }
    }

    private void UnhighlightPlayedHands()
    {
        StopPulsingAnimation();
        foreach (PlayedHandLogItemUI playedHandLogItem in m_PlayedHandLogItems)
        {
            playedHandLogItem.SetNormalTextColor();
        }
    }
}
