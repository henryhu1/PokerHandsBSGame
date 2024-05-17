using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EndOfGameUI : TransitionableUIBase
{
    public static EndOfGameUI Instance { get; private set; }

    [SerializeField] private ResultItemUI m_resultItemUIPrefab;
    [SerializeField] private GameObject m_resultContent;
    [SerializeField] private GameObject m_optionsUI;
    [SerializeField] private GameObject m_restartGameOption;
    [SerializeField] private Button m_gameModeButton;
    [SerializeField] private TextMeshProUGUI m_gameModeText;
    [SerializeField] private Button m_restartButton;
    [SerializeField] private Button m_exitButton;
    private GameType m_selectedGameType;
    private List<ResultItemUI> m_resultItems;

    [HideInInspector]
    public delegate void RestartGameDelegateHandler(GameType gameType);
    [HideInInspector]
    public event RestartGameDelegateHandler OnRestartGame;

    [HideInInspector]
    public delegate void ExitGameDelegateHandler();
    [HideInInspector]
    public event ExitGameDelegateHandler OnExitGame;

    protected override void Awake()
    {
        base.Awake();

        if (Instance != this && Instance != null)
        {
            Destroy(Instance.gameObject);
        }
        Instance = this;

        m_selectedGameType = GameManager.Instance.SelectedGameType;
        m_resultItems = new List<ResultItemUI>();

        m_gameModeButton.onClick.AddListener(() =>
        {
            switch (m_selectedGameType)
            {
                case GameType.Ascending:
                    m_selectedGameType = GameType.Descending;
                    m_gameModeText.text = GameType.Descending.ToString();
                    break;
                case GameType.Descending:
                    m_selectedGameType = GameType.Ascending;
                    m_gameModeText.text = GameType.Ascending.ToString();
                    break;
            }
        });

        m_restartButton.onClick.AddListener(() =>
        {
            OnRestartGame?.Invoke(m_selectedGameType);
        });

        m_exitButton.onClick.AddListener(() =>
        {
            OnExitGame?.Invoke();
        });

        m_optionsUI.gameObject.SetActive(GameManager.Instance.IsHost);
    }

    protected override void RegisterForEvents()
    {
        GameManager.Instance.RegisterEndOfGameUICallbacks();
        GameManager.Instance.OnGameWon += GameManager_GameWon;
        GameManager.Instance.OnRestartGame += GameManager_RestartGame;
    }

    protected override void Start()
    {
        RegisterForEvents();
        base.Start();
    }

    private void OnDestroy()
    {
        GameManager.Instance.UnregisterEndOfGameUICallbacks();
        GameManager.Instance.OnGameWon -= GameManager_GameWon;
        GameManager.Instance.OnRestartGame -= GameManager_RestartGame;
    }

    public void GameManager_GameWon(int myPosition, List<GameManager.PlayerData> eliminationOrder)
    {
        m_restartGameOption.gameObject.SetActive(GameManager.Instance.m_connectedClientIds.Count != 1);
        for (int i = 0; i < eliminationOrder.Count; i++)
        {
            GameManager.PlayerData playerData = eliminationOrder[i];
            string playerName = playerData.Name;
            if (i + 1 == myPosition)
            {
                playerName += " (you)";
            }
            ResultItemUI resultItemUI = Instantiate(m_resultItemUIPrefab, m_resultContent.transform);
            resultItemUI.GivePlacementItem(eliminationOrder.Count - i, playerName);

            m_resultItems.Add(resultItemUI);
        }
        StartAnimation();
    }

    public void GameManager_RestartGame()
    {
        foreach (ResultItemUI resultItemUI in m_resultItems) Destroy(resultItemUI.gameObject);
        m_resultItems.Clear();
        StartAnimation();
    }
}
