using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EndOfGameUI : MonoBehaviour
{
    public static EndOfGameUI Instance { get; private set; }

    [SerializeField] private ResultItemUI m_resultItemUIPrefab;
    [SerializeField] private GameObject m_resultContent;

    [Header("Host UI")]
    [SerializeField] private GameObject m_optionsUI;
    [SerializeField] private Button m_gameModeButton;
    [SerializeField] private TextMeshProUGUI m_gameModeText;
    [SerializeField] private Button m_restartButton;
    [SerializeField] private Button m_exitButton;
    private List<ResultItemUI> m_resultItems;
    private GameType selectedGameType;
    private TransitionableUIBase animatable;

    [Header("Firing Events")]
    [SerializeField] private VoidEventChannelSO OnRestartGame;
    [SerializeField] private VoidEventChannelSO OnExitGame;

    [Header("Listening Events")]
    [SerializeField] private VoidEventChannelSO OnInitializeNewGame;

    private void Awake()
    {
        if (Instance != this && Instance != null)
        {
            Destroy(Instance.gameObject);
        }
        Instance = this;

        m_resultItems = new List<ResultItemUI>();
        animatable = GetComponent<TransitionableUIBase>();
        
        m_gameModeButton.onClick.AddListener(() =>
        {
            switch (selectedGameType)
            {
                case GameType.Ascending:
                    selectedGameType = GameType.Descending;
                    m_gameModeText.text = GameType.Descending.ToString();
                    break;
                case GameType.Descending:
                    selectedGameType = GameType.Ascending;
                    m_gameModeText.text = GameType.Ascending.ToString();
                    break;
            }
        });

        m_restartButton.onClick.AddListener(() =>
        {
            var rules = GameSession.Instance.ActiveRules;
            GameRulesSO selectedRules = GameRulesFactory.CreateRuntime(rules);
            selectedRules.selectedGameType = selectedGameType;
            GameSession.Instance.SetRules(selectedRules);

            OnRestartGame.RaiseEvent();
        });

        m_exitButton.onClick.AddListener(() =>
        {
            OnExitGame.RaiseEvent();
        });
    }

    private void Start()
    {
        selectedGameType = GameSession.Instance.ActiveRules.selectedGameType;
        m_gameModeText.text = selectedGameType == GameType.Descending ? GameType.Descending.ToString() : GameType.Ascending.ToString();
    }

    private void OnEnable()
    {
        OnInitializeNewGame.OnEventRaised += InitializeNewGame;
    }

    private void OnDisable()
    {
        OnInitializeNewGame.OnEventRaised -= InitializeNewGame;
    }

    public void DisplayGameResults(int myPosition, List<PlayerData> eliminationOrder)
    {
        m_optionsUI.SetActive(GameManager.Instance.IsHost);

        m_restartButton.gameObject.SetActive(eliminationOrder.Count > 1);
        for (int i = 0; i < eliminationOrder.Count; i++)
        {
            PlayerData playerData = eliminationOrder[i];
            string playerName = playerData.GetName();
            if (i + 1 == myPosition)
            {
                playerName += " (you)";
            }
            ResultItemUI resultItemUI = Instantiate(m_resultItemUIPrefab, m_resultContent.transform);
            resultItemUI.GivePlacementItem(eliminationOrder.Count - i, playerName);

            m_resultItems.Add(resultItemUI);
        }
        animatable.TransitionOnToScreen();
    }

    private void InitializeNewGame()
    {
        foreach (ResultItemUI resultItemUI in m_resultItems) Destroy(resultItemUI.gameObject);
        m_resultItems.Clear();
        animatable.TransitionOffScreen();
    }
}
