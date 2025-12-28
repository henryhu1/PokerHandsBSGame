using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EndOfGameUI : MonoBehaviour
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
    private TransitionableUIBase animatable;

    [HideInInspector]
    public delegate void ExitGameDelegateHandler();
    [HideInInspector]
    public event ExitGameDelegateHandler OnExitGame;

    [Header("Firing Events")]
    [SerializeField] private IntEventChannelSO OnRestartGame;

    [Header("Listening Events")]
    [SerializeField] private VoidEventChannelSO OnInitializeNewGame;

    private void Awake()
    {
        if (Instance != this && Instance != null)
        {
            Destroy(Instance.gameObject);
        }
        Instance = this;

        m_selectedGameType = GameManager.Instance.GetGameType();
        m_resultItems = new List<ResultItemUI>();
        animatable = GetComponent<TransitionableUIBase>();

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
            OnRestartGame.RaiseEvent((int)m_selectedGameType);
        });

        m_exitButton.onClick.AddListener(() =>
        {
            OnExitGame?.Invoke();
        });

        m_optionsUI.SetActive(GameManager.Instance.IsHost);
    }

    private void OnEnable()
    {
        OnInitializeNewGame.OnEventRaised += InitializeNewGame;
    }

    private void OnDisable()
    {
        OnInitializeNewGame.OnEventRaised -= InitializeNewGame;
    }

    private void Start()
    {
        GameManager.Instance.RegisterEndOfGameUICallbacks();
    }

    private void OnDestroy()
    {
        GameManager.Instance.UnregisterEndOfGameUICallbacks();
    }

    public void DisplayGameResults(int myPosition, List<PlayerData> eliminationOrder)
    {
        m_restartGameOption.SetActive(GameManager.Instance.m_connectedClientIds.Count != 1);
        for (int i = 0; i < eliminationOrder.Count; i++)
        {
            PlayerData playerData = eliminationOrder[i];
            string playerName = playerData.Name;
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
