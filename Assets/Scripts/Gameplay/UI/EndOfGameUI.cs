using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EndOfGameUI : MonoBehaviour
{
    public static EndOfGameUI Instance { get; private set; }

    [SerializeField] private ResultItemUI m_resultItemUIPrefab;
    [SerializeField] private GameObject m_resultContent;
    [SerializeField] private GameObject m_restartGameUI;
    [SerializeField] private Button m_gameModeButton;
    [SerializeField] private TextMeshProUGUI m_gameModeText;
    [SerializeField] private Button m_restartButton;
    private PokerHandsBullshitGame.GameType m_selectedGameType;
    private List<ResultItemUI> m_resultItems;

    [HideInInspector]
    public delegate void RestartGameDelegateHandler(PokerHandsBullshitGame.GameType gameType);
    [HideInInspector]
    public event RestartGameDelegateHandler OnRestartGame;

    private void Awake()
    {
        Instance = this;

        m_selectedGameType = PokerHandsBullshitGame.Instance.SelectedGameType;

        m_gameModeButton.onClick.AddListener(() =>
        {
            switch (m_selectedGameType)
            {
                case PokerHandsBullshitGame.GameType.Ascending:
                    m_selectedGameType = PokerHandsBullshitGame.GameType.Descending;
                    m_gameModeText.text = PokerHandsBullshitGame.GameType.Descending.ToString();
                    break;
                case PokerHandsBullshitGame.GameType.Descending:
                    m_selectedGameType = PokerHandsBullshitGame.GameType.Ascending;
                    m_gameModeText.text = PokerHandsBullshitGame.GameType.Ascending.ToString();
                    break;
            }
        });

        m_restartButton.onClick.AddListener(() =>
        {
            OnRestartGame?.Invoke(m_selectedGameType);
        });

        m_restartGameUI.gameObject.SetActive(PokerHandsBullshitGame.Instance.IsHost);
    }

    private void Start()
    {
        PokerHandsBullshitGame.Instance.OnGameWon += GameManager_GameWon;
        PokerHandsBullshitGame.Instance.OnRestartGame += GameManager_RestartGame;
    }

    private void OnDisable()
    {
        PokerHandsBullshitGame.Instance.OnGameWon -= GameManager_GameWon;
        PokerHandsBullshitGame.Instance.OnRestartGame -= GameManager_RestartGame;
    }

    public void GameManager_GameWon(int myPosition, List<PokerHandsBullshitGame.PlayerData> eliminationOrder)
    {
        for (int i = 0; i < eliminationOrder.Count; i++)
        {
            PokerHandsBullshitGame.PlayerData playerData = eliminationOrder[i];
            string playerName = playerData.Name;
            if (i == myPosition)
            {
                playerName += " (you)";
            }
            ResultItemUI resultItemUI = Instantiate(m_resultItemUIPrefab, m_resultContent.transform);
            resultItemUI.GivePlacementItem(i, playerName);

            m_resultItems.Add(resultItemUI);
        }
    }

    public void GameManager_RestartGame()
    {
        foreach (ResultItemUI resultItemUI in m_resultItems) Destroy(resultItemUI.gameObject);
        m_resultItems.Clear();
    }
}
